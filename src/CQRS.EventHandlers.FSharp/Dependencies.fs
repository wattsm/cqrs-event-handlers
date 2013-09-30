namespace CQRS.EventHandlers.FSharp

open System
open System.Collections.Generic
open CQRS.EventHandlers

module Dependencies = 

    type Dependency = {
        EventType : Guid;
        Check : IEvent -> Revision option;
        Versions : IVersionProvider;     
    }
    with

        static member On<'TEvent when 'TEvent :> IEvent> (versions, check : 'TEvent -> Revision option) = 

            let check' (event : IEvent) = 
                let event' = event :?> 'TEvent
                in check event'

            {
                EventType = typeof<'TEvent>.GUID;
                Check = check';
                Versions = versions;
            }

        //C# interop
        static member On<'TEvent when 'TEvent :> IEvent> (versions, check : Func<'TEvent, Revision>) = 
            
            let check' event =
                match (check.Invoke event) with
                | null -> None
                | revision -> Some revision

            Dependency.On<'TEvent> (versions, check')
        

    type DependencyState = 
        | Satisfied
        | Unsatisfied of Revision

    let getDependencyState event dependency = 
        match (dependency.Check event) with
        | Some revision -> 
            if (dependency.Versions.GetVersion (revision.Id) >= revision.Version) then
                Satisfied
            else
                Unsatisfied revision
        | _ -> Satisfied

    let getUnsatisfiedDependency (dependenciesByEvent : IDictionary<Guid, Dependency seq>) event = 

        let eventType = event.GetType ()
        
        match (dependenciesByEvent.TryGetValue eventType.GUID) with
        | (false, _) -> None
        | (_, dependencies) ->
            dependencies
            |> Seq.tryPick (fun dependency ->
                    match (getDependencyState event dependency) with
                    | Unsatisfied revision -> Some (dependency.Versions, revision)
                    | _ -> None
                )

    type Decorator<'TEntity when 'TEntity : not struct and 'TEntity :> IEntity> (wrapped : IEventHandler<'TEntity>, dependencies : Dependency seq) = 

        let dependenciesByEvent = 
            dependencies
            |> Seq.groupBy (fun d -> d.EventType)
            |> dict

        static member Decorate (wrapped, dependencies) = 
            new Decorator<'TEntity> (wrapped, dependencies) :> IEventHandler<'TEntity>

        member private this.Service = 
            this :> IEventHandler
        
        interface IEventHandler<'TEntity> with

            member this.HandleEvent event = 
                match (getUnsatisfiedDependency dependenciesByEvent event) with
                | Some (versions, revision) ->

                    versions.Subscribe (
                        revision, 
                        fun () -> this.Service.HandleEvent event
                    )

                | _ -> 
                    wrapped.HandleEvent event

            member this.Repository 
                with get () = wrapped.Repository

            member this.Dispose () = 
                if (wrapped <> null) then
                    wrapped.Dispose ()


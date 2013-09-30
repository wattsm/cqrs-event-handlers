namespace CQRS.EventHandlers.FSharp

open System

module Helpers = 

    module Tasks =

        open System.Threading
        open System.Threading.Tasks

        let start token computation = 
            Async.StartAsTask (computation, cancellationToken = token)

    module Collections = 

        open System.Collections.Concurrent

        let take<'T when 'T : null> (collection : BlockingCollection<'T>) =
            try
                match (collection.Take ()) with
                | null -> None
                | item -> Some item
            with
            | :? InvalidOperationException -> None //Adding has finished
            | :? ArgumentNullException -> None //Object disposed

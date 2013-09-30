### CQRS Event Handlers

In a CQRS system there are several challenges to handling events arriving at the read model. This repository contains examples of potential solutions to some of those problems. 

I have provided example solutions in C# and F#, although the common bits and pieces (interfaces, model etc.) are in C#.

#### Basics

The **CQRS.EventHandlers** project contains code defining base classes and interfaces for the read model and the various artefacts associated with it. The main points of interest
here are the ``IEventHandler`` and ``IEventHandler<T>`` interfaces as they are the primary point of contact between your read model and the write model. As the write model is updated
it will generate events which are then consumed by the read model to bring itself to the corresponding state.

The **CQRS.EventHandlers.App** project is a console application which builds a very simple read model on these foundations and then demonstrates how the solutions described below can help
process a stream of (potentially out of order) events.

The solutions all take the form of decorators for the ``IEventHandler<T>`` interface which can be selectively applied. 

C# decorators can be found in **CQRS.EventHandlers.CSharp\Behaviours** . F# decorators can be found in **CQRS.EventHandlers.FSharp**. Tests are defined in **CQRS.EventHandlers.Tests**, which 
C# and F# implementations found in **CQRS.EventHandlers.CSharp.Tests** and **CQRS.EventHandlers.FSharp.Tests** respectively.

#### Event ordering

One of first challenges of creating a read model is how to deal with events that, for whatever reason, arriving out of order - e.g. ChangeCustomerName arrives before CreateCustomer or similar.
If we assume that all of our events are given an entity-specific version number then one strategy is to simply add events to a backlog if they arrive out of order and then process them once
the model is ready for them.

For example if an event stream for an entity consisted of events with versions 1, 3, 5, 2, 4 then to process them we would:

* Event 1 arrives - apply (version = 1, backlog = [])
* Event 3 arrives - add to backlog (version = 1, backlog = [3])
* Event 5 arrives - add to backlog (version = 1, backlog = [3,5])
* Event 2 arrives - apply (version = 2, backlog = [3,5])
  * Apply event 3 from backlog (version = 3, backlog = [5])
* Event 4 arrives - apply (version = 4, backlog = [5])
  * Apply event 5 from backlog (version = 5, backlog = [])
  
The ordered decorator can be used to add this behaviour to an event handler. The C# decorator can be found in **Ordered\Decorator.cs** and the F# equivalent is
in **Ordered.fs**. 
  
The snippets below show how to apply this behaviour to some hypothetical event handler, ``WidgetHandler``, which implements ``IEventHandler<T>``.

```csharp
public static IEventHandler<Widget> Build(IRepository<Widget> widgets) { 
  return WidgetHandler
          .Create(widgets)
          .Ordered();
}
```

```fsharp
let build widgets = 
  widgets
  |> WidgetHandler.Create
  |> Ordered.Decorator.Decorate
```

#### Concurrency

Depending on how your application is structured there's a good chance that events will be pushed to your read model by multiple threads. This can make things tricky as you have to 
make sure that only one thread can update an entity at once or you risk all manner of concurrency related gremlins.

The concurrent decorator can be used to funnel events that arrive from multiple threads onto a single thread for processing using the ``BlockingCollection<T>`` and a long running
``Task``. Events are simply added to the ``BlockingCollection<T>`` by the calling thread and then the ``Task`` continually loops, processing the events one at a time. This means
version checks, updates etc. are guaranteed to be performed sequentially, by a single thread.

The C# decorator can be found in **Concurrent\Decorator.cs** and the F# equivalent is in **Concurrent.fs**.

Again, the snippets below show how this behaviour can be added to the ``WidgetHandler``.

```csharp
public static IEventHandler<Widget> Build(IRepository<Widget> widgets) {
  return WidgetHandler
          .Create(widgets)
          .Concurrent();
}
```

```fsharp
let build widgets = 
  widgets
  |> WidgetHandler.Create
  |> Concurrent.Decorator.Decorate
```

#### Dependencies

This problem is related to events arriving out of order. If entity A relies on the existence of entity B then you may not be able to create or update entity A until entity B has been created.

The dependency decorator allows you to specify any dependencies for a given event. If those dependencies are not satisfied then the decorator will register a callback with the 
appropriate repository which will be invoked when the dependency is satisified. This callback will simply retry the event.

For example, the event stream for entity A might look like 1, 2 (depends on B version 1). The process for these events would be:

* Event 1 arrives - apply
* Event 2 arrives (unsatisified) - register callback with repository of Bs
* (B version 1 is created, repository invokes callback)
* Event 2 arrives (satisified) - apply
  
The C# decorator can be found in **Dependencies\Decorator.cs** and the F# equivalent is in **Dependencies.fs**.

The snippets below show how this behaviour can be added to the ``WidgetHandler``.

```csharp
public static IEventHandler<Widget> Build(IRepository<Widget> widgets, IRepository<Thingy> thingies) {
  return WidgetHandler
          .Create(widgets)
          .WithDependencies()
          .On<WidgetCreated>(thingies, @event => Revision.Create(@event.ThingyId));
}
```

```fsharp
let build widgets thingies = 

  let dependencies = 
    let check = fun event -> Some (Revision.Create event.ThingyId)
    in
      Dependencies.Dependency.On<WidgetCreated> (thingies, check)
      |> Seq.singleton
  
  let handler =
    WidgetHandler.Create (widgets)
    
  Dependencies.Decorator.Decorate (handler, dependencies)
```

#### Combining behaviours

Due to the nature of some of these behaviours the order in which they are applied is important. The best ordering is ordering -> concurrency -> dependencies, which gives the
following combined behaviour:

* Dependencies are checked.
* Events are funnelled onto a single thread.
* Event ordering is enforced.

It is desirable to check dependencies before funnelling events onto a single thread for processing as the callbacks registered when a dependency is not satisified will be 
made on new threads, thus reintroducing potential concurrency issues during processing. Dependency checks are quick and even if race conditions do mean a dependency is incorrectly 
flagged as unsatisfied the callback will be raised almost immediately once registered.

Once dependencies have been checked the events can be funnelled into a single thread and then either processed in order.
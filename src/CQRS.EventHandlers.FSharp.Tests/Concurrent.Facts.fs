namespace CQRS.EventHandlers.FSharp.Tests

open System
open Xunit
open CQRS.EventHandlers.FSharp
open CQRS.EventHandlers.Tests

module ``Concurrent facts`` = 

    [<Trait (Constants.Category, Constants.FSharp)>]
    type ``Concurrent Decorator HandleEvent facts`` () = 
        inherit Concurrent_Decorator_HandleEvent_Facts ()

        override this.Decorate handler = 
            Concurrent.Decorator.Decorate handler
namespace CQRS.EventHandlers.FSharp.Tests

open System
open Xunit
open CQRS.EventHandlers.FSharp
open CQRS.EventHandlers.Tests

module ``Ordered facts`` = 

    [<Trait (Constants.Category, Constants.FSharp)>]
    type ``Ordered Decorator HandleEvent facts`` () = 
        inherit Ordered_Decorator_HandleEvent_Facts ()

        override this.Decorate handler = 
            Ordered.Decorator.Decorate handler
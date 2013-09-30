using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.EventHandlers.CSharp.Behaviours.Ordered;
using Moq;
using Xunit;
using Base = CQRS.EventHandlers.Tests;

namespace CQRS.EventHandlers.CSharp.Tests {
    [Trait(Base.Constants.Category, Base.Constants.CSharp)]
    public class Ordered_Decorator_HandleEvent_Facts : Base.Ordered_Decorator_HandleEvent_Facts {
        protected override IEventHandler<Base.FakeEntity> Decorate(IEventHandler<Base.FakeEntity> handler) {
            return new Decorator<Base.FakeEntity>(handler);
        }
    }
}

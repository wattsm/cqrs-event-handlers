using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.EventHandlers.CSharp.Behaviours.Dependencies;
using Moq;
using Xunit;
using Base = CQRS.EventHandlers.Tests;

namespace CQRS.EventHandlers.CSharp.Tests {
    [Trait(Base.Constants.Category, Base.Constants.CSharp)]
    public class Dependencies_Decorator_HandleEvent_Facts : Base.Dependencies_Decorator_HandleEvent_Facts {
        protected override IEventHandler<Base.FakeEntity> Decorate(IEventHandler<Base.FakeEntity> handler, IVersionProvider versions, Func<Base.FakeEvent, Revision> check) {

            var decorated = new Decorator<Base.FakeEntity>(handler);

            if(versions != null && check != null) {
                decorated.On(versions, check);
            }

            return decorated;
        }
    }
}

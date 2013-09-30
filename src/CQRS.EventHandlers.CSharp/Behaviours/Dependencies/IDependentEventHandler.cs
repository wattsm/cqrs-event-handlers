using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Dependencies {
    public interface IDependentEventHandler<TEntity> : IEventHandler<TEntity> where TEntity : class, IEntity {

        IDependentEventHandler<TEntity> On<TEvent>(IVersionProvider versions, Func<TEvent, Revision> check) where TEvent : IEvent;

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {

    public interface IEventHandler {

        void HandleEvent(IEvent @event);

    }

    public interface IEventHandler<TEntity> : IEventHandler, IDisposable where TEntity : class, IEntity {

        IRepository<TEntity> Repository { get; }

    }
}

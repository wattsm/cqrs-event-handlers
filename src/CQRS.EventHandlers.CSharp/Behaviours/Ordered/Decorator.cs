using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Ordered {
    public class Decorator<TEntity> : IEventHandler<TEntity> where TEntity : class, IEntity {

        private readonly IEventHandler<TEntity> _wrapped;
        private readonly BacklogManager _backlogs;

        public Decorator(IEventHandler<TEntity> wrapped) {
            _wrapped = wrapped;            
            _backlogs = new BacklogManager();
        }

        public void HandleEvent(IEvent @event) {
            if(this.CanApply(@event)) {

                _wrapped.HandleEvent(@event);

                _backlogs.HandleSuccessorsOf(
                    @event.Revision,
                    _wrapped
                );

            } else {
                _backlogs.Enqueue(@event);
            }
        }        

        public void Dispose() {
            if(_wrapped != null) {
                _wrapped.Dispose();
            }
        }

        private bool CanApply(IEvent @event) {
            return this.Repository.GetVersion(@event.Revision.Id) == (@event.Revision.Version - 1);
        }

        public IRepository<TEntity> Repository { get { return _wrapped.Repository; } }
    }
}

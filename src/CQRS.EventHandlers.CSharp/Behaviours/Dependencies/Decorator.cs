using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Dependencies {
    public class Decorator<TEntity> : IDependentEventHandler<TEntity> where TEntity : class, IEntity {

        private readonly IEventHandler<TEntity> _wrapped;
        private readonly Dictionary<Guid, List<IDependencyManager>> _dependencies = new Dictionary<Guid, List<IDependencyManager>>();

        public Decorator(IEventHandler<TEntity> wrapped) {
            _wrapped = wrapped;
        }

        public IDependentEventHandler<TEntity> On<TEvent>(IVersionProvider versions, Func<TEvent, Revision> check) where TEvent : IEvent {

            var typeId = typeof(TEvent).GUID;

            if(!_dependencies.ContainsKey(typeId)) {
                _dependencies.Add(typeId, new List<IDependencyManager>());
            }

            _dependencies[typeId].Add(new DependencyManager<TEvent>(versions, check));

            return this;
        }

        #region IEventHandler members

        public IRepository<TEntity> Repository {
            get { return _wrapped.Repository; }
        }

        public void HandleEvent(IEvent @event) {

            var typeId = @event.GetType().GUID;
            var canHandle = true;            

            if(_dependencies.ContainsKey(typeId)) {

                Action callback = () => this.HandleEvent(@event);

                foreach(var dependency in _dependencies[typeId]) {
                    if(!dependency.IsSatisfied(@event, callback)) {
                        canHandle = false;
                        break;
                    }
                }
            }

            if(canHandle) {
                _wrapped.HandleEvent(@event);
            }
        }

        #endregion

        #region IDisposable members

        public void Dispose() {
            if(_wrapped != null) {
                _wrapped.Dispose();
            }
        }

        #endregion
    }
}

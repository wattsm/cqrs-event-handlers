using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public abstract class BaseEventHandler<TEntity> : IEventHandler<TEntity> where TEntity : class, IEntity {

        private readonly IRepository<TEntity> _repository;
        private readonly Dictionary<Guid, Action<IEvent>> _handlers = new Dictionary<Guid, Action<IEvent>>();

        protected BaseEventHandler(IRepository<TEntity> repository) {
            _repository = repository;
        }

        protected void RegisterHandler<TEvent>(Action<TEvent> handler) where TEvent : IEvent {
            _handlers.Add(
                typeof(TEvent).GUID,
                @event => handler((TEvent)@event)
            );
        }

        protected void Update(long id, Action<TEntity> update) {

            var entity = this.Repository.Get(id);

            update(entity);

            this.Repository.Save(entity);
        }

        #region IEventHandler members

        public void HandleEvent(IEvent @event) {

            var typeId = @event.GetType().GUID;

            if(_handlers.ContainsKey(typeId)) {
                _handlers[typeId].Invoke(@event);
            } else {
                throw new NotSupportedException();
            }
        }

        public IRepository<TEntity> Repository {
            get { return _repository; }
        }

        #endregion

        #region IDisposable members

        public void Dispose() {
        }

        #endregion
    }
}

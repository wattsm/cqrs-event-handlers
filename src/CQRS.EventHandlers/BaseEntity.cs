using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public abstract class BaseEntity : IEntity {

        private Revision _revision;
        private readonly List<IEvent> _history = new List<IEvent>();

        protected BaseEntity(IEvent @event) {
            _revision = @event.Revision;
        }

        protected void Update(IEvent @event, Action update) {

            update();

            _revision = @event.Revision;
            _history.Add(@event);
        }

        public Revision Revision { get { return _revision; } }
    }
}

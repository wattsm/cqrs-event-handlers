using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public abstract class BaseEvent : IEvent {

        private readonly Revision _revision;

        protected BaseEvent(Revision revision) {
            _revision = revision;
        }

        protected BaseEvent(long id)
            : this(Revision.Create(id)) {
        }

        public Revision Revision {
            get { return _revision; } 
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Tests {    
    public class FakeEntity : IEntity {
        #region IVersioned Members

        public Revision Revision {
            get { return Revision.Empty; }
        }

        #endregion
    }

    public class FakeEvent : BaseEvent {
        public FakeEvent()
            : base(1L) {
        }
    }
}

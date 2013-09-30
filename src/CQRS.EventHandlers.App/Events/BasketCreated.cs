using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Events {
    public class BasketCreated : BaseEvent {

        private readonly string _description;

        public BasketCreated(long id, string description)
            : base(id) {

                _description = description; 
        }

        public override string ToString() {
            return String.Format("{0} create basket", this.Revision);
        }

        public string Description { get { return _description; } }
    }
}

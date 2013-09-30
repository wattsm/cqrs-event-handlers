using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Ordered {
    public class Backlog  {

        private readonly Dictionary<long, IEvent> _events = new Dictionary<long, IEvent>();

        public void Enqueue(IEvent @event) {
            _events.Add(@event.Revision.Version, @event);
        }

        public bool IsEmpty { get { return _events.Count == 0; } } 

        public void HandleSuccessorsOf(long version, IEventHandler handler) {

            version++;

            while(_events.ContainsKey(version)) {
                handler.HandleEvent(_events[version]);
                _events.Remove(version);
                version++;
            }
        }
    }
}

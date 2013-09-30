using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Ordered {
    public class BacklogManager {

        private readonly Dictionary<long, Backlog> _backlogs = new Dictionary<long, Backlog>();

        public void Enqueue(IEvent @event) {

            var id = @event.Revision.Id;

            if(!_backlogs.ContainsKey(id)) {
                _backlogs.Add(id, new Backlog());
            }

            _backlogs[id].Enqueue(@event);
        }

        public void HandleSuccessorsOf(Revision revision, IEventHandler handler) {
            if(_backlogs.ContainsKey(revision.Id)) {

                var backlog = _backlogs[revision.Id];

                backlog.HandleSuccessorsOf(revision.Version, handler);

                if(backlog.IsEmpty) {
                    _backlogs.Remove(revision.Id);
                }
            }
        }
    }
}

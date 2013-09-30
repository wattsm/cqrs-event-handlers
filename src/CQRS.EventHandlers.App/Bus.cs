using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public class Bus {

        private readonly Dictionary<Guid, IEventHandler> _bindings = new Dictionary<Guid, IEventHandler>();

        public void Bind<TEvent>(IEventHandler handler) where TEvent : IEvent {
            _bindings.Add(typeof(TEvent).GUID, handler);
        }

        public void Publish(IEvent @event) {

            //Debug.WriteLine("[{0}] Publishing {1}", Thread.CurrentThread.ManagedThreadId, @event);

            var typeId = @event.GetType().GUID;
            var handler = _bindings[typeId];

            Task.Run(() => handler.HandleEvent(@event));
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Concurrent {
    public class Decorator<TEntity> : IEventHandler<TEntity> where TEntity : class, IEntity {

        private readonly IEventHandler<TEntity> _wrapped;
        private readonly BlockingCollection<IEvent> _events = new BlockingCollection<IEvent>();
        private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
        private bool _disposed = false;

        public Decorator(IEventHandler<TEntity> wrapped) {
            _wrapped = wrapped;

            //TODO Is task a good fit here, is a thread a better option?

            Task.Run(
                () => {
                    while(!_disposed) {

                        IEvent @event = null;

                        try {
                            @event = _events.Take();
                        } catch(ArgumentNullException) {
                            //Note collection disposed after check against _disposed
                        } catch(InvalidOperationException) {
                            //Note collection completed after check against _disposed
                        }

                        if(@event != null) {
                            _wrapped.HandleEvent(@event);
                        }
                    }
                },
                _cancel.Token
            );
        }

        public void HandleEvent(IEvent @event) {
            _events.Add(@event);
        }

        #region IEventHandler members

        public IRepository<TEntity> Repository { get { return _wrapped.Repository; } }

        #endregion

        #region IDisposable members

        public void Dispose() {

            _disposed = true;

            if(_cancel != null) {
                _cancel.Cancel();
                _cancel.Dispose();
            }

            if(_events != null) {
                _events.CompleteAdding();
                _events.Dispose();
            }

            if(_wrapped != null) {
                _wrapped.Dispose();
            }
        }

        #endregion
    }
}

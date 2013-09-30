using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public abstract class BaseVersionProvider : IVersionProvider {

        private readonly ConcurrentDictionary<Revision, List<Action>> _callbacks = new ConcurrentDictionary<Revision, List<Action>>();

        public void Subscribe(Revision revision, Action callback) {
            if(this.GetVersion(revision.Id) >= revision.Version) {
                Task.Run(callback);
            } else {
                _callbacks.AddOrUpdate(
                    revision,
                    BaseVersionProvider.GetAdd(callback),
                    BaseVersionProvider.GetUpdate(callback)
                );
            }
        }

        public abstract long GetVersion(long id);

        #region Helpers

        protected void Notify(Revision revision) {

            List<Action> callbacks;

            if(_callbacks.TryRemove(revision, out callbacks)) {
                foreach(var callback in callbacks) {
                    Task.Run(callback);
                }
            }
        }

        private static Func<Revision, List<Action>> GetAdd(Action callback) {
            return revision => {

                var callbacks = new List<Action>();
                callbacks.Add(callback);

                return callbacks;
            };
        }

        private static Func<Revision, List<Action>, List<Action>> GetUpdate(Action callback) {
            return (revision, existing) => {

                existing.Add(callback);

                return existing;
            };
        }

        #endregion
    }
}

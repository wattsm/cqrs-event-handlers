using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Dependencies {
    public class DependencyManager<TEvent> : IDependencyManager where TEvent : IEvent {

        private readonly IVersionProvider _versions;
        private readonly Func<TEvent, Revision> _check;

        public DependencyManager(IVersionProvider versions, Func<TEvent, Revision> check) {
            _versions = versions;
            _check = check;
        }

        public bool IsSatisfied(IEvent @event, Action callback) {
            return this.IsSatisfied((TEvent)@event, callback);
        }

        private bool IsSatisfied(TEvent @event, Action callback) {

            var revision = _check(@event);
            var satisified = true;

            if(!Revision.IsNullOrEmpty(revision)) {
                if(_versions.GetVersion(revision.Id) < revision.Version) {

                    //TODO This needs to be the last behaviour added, or at least "above" concurrency.

                    _versions.Subscribe(revision, callback);

                    satisified = false;
                }
            }

            return satisified;
        }
    }
}

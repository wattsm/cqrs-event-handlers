using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.CSharp.Behaviours.Dependencies {
    public interface IDependencyManager {

        bool IsSatisfied(IEvent @event, Action callback);

    }
}

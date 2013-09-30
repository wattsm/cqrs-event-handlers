using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public interface IVersionProvider {

        void Subscribe(Revision revision, Action callback);

        long GetVersion(long id);

    }
}

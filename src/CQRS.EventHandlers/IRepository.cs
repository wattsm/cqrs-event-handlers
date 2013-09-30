using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public interface IRepository<TEntity> : IVersionProvider, IDisposable where TEntity : class, IEntity {

        void Save(TEntity entity);

        TEntity Get(long id);

        IEnumerable<TEntity> All();

    }
}

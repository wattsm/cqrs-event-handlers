using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers {
    public class InMemoryRepository<TEntity> : BaseVersionProvider, IRepository<TEntity> where TEntity : class, IEntity {

        private readonly Dictionary<long, TEntity> _entities = new Dictionary<long, TEntity>();

        #region IRepository members

        public override long GetVersion(long id) {

            var entity = this.Get(id);

            return entity == null 
                    ? 0L 
                    : entity.Revision.Version;
        }

        public void Save(TEntity entity) {

            _entities[entity.Revision.Id] = entity;

            this.Notify(entity.Revision);
        }

        public TEntity Get(long id) {
            return _entities.ContainsKey(id)
                    ? _entities[id]
                    : null;
        }

        public IEnumerable<TEntity> All() {
            return _entities.Values.ToArray();
        }

        #endregion

        #region IDisposable members

        public void Dispose() {
        }

        #endregion
    }
}

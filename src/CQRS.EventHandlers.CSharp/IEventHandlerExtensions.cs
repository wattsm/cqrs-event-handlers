using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ord = CQRS.EventHandlers.CSharp.Behaviours.Ordered;
using Con = CQRS.EventHandlers.CSharp.Behaviours.Concurrent;
using Dep = CQRS.EventHandlers.CSharp.Behaviours.Dependencies;

namespace CQRS.EventHandlers.CSharp {
    public static class IEventHandlerExtensions {

        public static IEventHandler<TEntity> Ordered<TEntity>(this IEventHandler<TEntity> handler) where TEntity : class, IEntity {
            return new Ord.Decorator<TEntity>(handler);
        }

        public static IEventHandler<TEntity> Concurrent<TEntity>(this IEventHandler<TEntity> handler) where TEntity : class, IEntity {
            return new Con.Decorator<TEntity>(handler);
        }
        
        public static Dep.IDependentEventHandler<TEntity> WithDependencies<TEntity>(this IEventHandler<TEntity> handler) where TEntity : class, IEntity {
            return new Dep.Decorator<TEntity>(handler);
        }
    }
}

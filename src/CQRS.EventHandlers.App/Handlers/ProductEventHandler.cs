using CQRS.EventHandlers.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CQRS.EventHandlers.CSharp;

namespace CQRS.EventHandlers.Handlers {
    public class ProductEventHandler : BaseEventHandler<Product> {

        private ProductEventHandler(IRepository<Product> products)
            : base(products) {

                this.RegisterHandler<Events.ProductCreated>(this.OnCreate);
        }

        private static IEventHandler<Product> Create(IRepository<Product> products) {
            return new ProductEventHandler(products);
        }

        public static IEventHandler<Product> Build(IRepository<Product> products) {
            return ProductEventHandler
                        .Create(products)
                        .Ordered()
                        .Concurrent();
        }

        private void OnCreate(Events.ProductCreated @event) {

            this.LogApply(@event);

            this.Repository.Save(
                Product.BasedOn(@event)
            );
        }

        private void LogApply(IEvent @event) {
            //Debug.WriteLine("[{0}] Applying {1}", Thread.CurrentThread.ManagedThreadId, @event);
        }
    }
}

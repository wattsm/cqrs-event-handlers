using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.EventHandlers.Model;
using CQRS.EventHandlers.Events;
using System.Diagnostics;
using System.Threading;
using CQRS.EventHandlers.CSharp;

namespace CQRS.EventHandlers.Handlers {
    public class BasketEventHandler : BaseEventHandler<Basket> {

        private readonly IRepository<Product> _products;

        private BasketEventHandler(IRepository<Basket> baskets, IRepository<Product> products)
            : base(baskets) {

                _products = products;

                this.RegisterHandler<BasketCreated>(this.OnCreate);
                this.RegisterHandler<BasketProductAdded>(this.OnProductAdded);
                this.RegisterHandler<BasketProductQuantityChanged>(this.OnProductQuantityChanged);
                this.RegisterHandler<BasketProductRemoved>(this.OnProductRemoved);
        }

        private static IEventHandler<Basket> Create(IRepository<Basket> baskets, IRepository<Product> products) {
            return new BasketEventHandler(baskets, products);
        }

        public static IEventHandler<Basket> Build(IRepository<Basket> baskets, IRepository<Product> products) {
            return BasketEventHandler
                        .Create(baskets, products)
                        .Ordered()
                        .Concurrent()
                        .WithDependencies()
                        .On<BasketProductAdded>(products, @event => Revision.Create(@event.ProductId));
        }

        private void OnCreate(BasketCreated @event) {

            this.LogApply(@event);

            this.Repository.Save(
                Basket.BasedOn(@event)
            );
        }

        private void OnProductAdded(BasketProductAdded @event) {

            this.LogApply(@event);

            this.Update(
                @event.Revision.Id, 
                basket => basket.On(@event, _products)
            );
        }

        private void OnProductQuantityChanged(BasketProductQuantityChanged @event) {

            this.LogApply(@event);

            this.Update(
                @event.Revision.Id,
                basket => basket.On(@event)
            );
        }

        private void OnProductRemoved(BasketProductRemoved @event) {

            this.LogApply(@event);

            this.Update(
                @event.Revision.Id,
                basket => basket.On(@event)
            );
        }

        private void LogApply(IEvent @event) {
            //Debug.WriteLine("[{0}] Applying {1}", Thread.CurrentThread.ManagedThreadId, @event);
        }
    }
}

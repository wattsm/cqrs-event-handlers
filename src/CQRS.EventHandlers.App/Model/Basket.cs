using CQRS.EventHandlers.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Model {
    public class Basket : BaseEntity {

        private readonly List<BasketProduct> _products = new List<BasketProduct>();
        private readonly string _description;

        private Basket(Events.BasketCreated @event)
            : base(@event) {

                _description = @event.Description;
        }

        public static Basket BasedOn(BasketCreated @event) {
            return new Basket(@event);
        }

        public void On(Events.BasketProductAdded @event, IRepository<Product> products) {
            this.Update(
                @event,
                () => {
                    _products.Add(
                        new BasketProduct(
                            products.Get(@event.ProductId), 
                            @event.Quantity
                        )
                    );
                }
            );
        }

        public void On(BasketProductQuantityChanged @event) {
            this.Update(
                @event,
                () => {

                    var product = this.FindProduct(@event.ProductId);
                    product.Quantity = @event.Quantity;

                }
            );
        }

        public void On(BasketProductRemoved @event) {
            this.Update(
                @event,
                () => {

                    _products.Remove(
                        this.FindProduct(@event.ProductId)
                    );

                }
            );
        }

        #region Helpers

        private BasketProduct FindProduct(long productId) {
            return _products.FirstOrDefault(p => p.Product.Revision.Id == productId);
        }

        #endregion

        #region Properties

        public string Description { get { return _description; } }
        public IEnumerable<BasketProduct> Products { get { return _products; } }

        #endregion
    }
}

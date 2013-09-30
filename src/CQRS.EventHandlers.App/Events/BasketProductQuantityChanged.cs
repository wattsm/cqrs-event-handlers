using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Events {
    public class BasketProductQuantityChanged : BaseEvent {

        private readonly long _productId;
        private readonly int _quantity;

        public BasketProductQuantityChanged(Revision revision, long productId, int quantity)
            : base(revision) {

                _productId = productId;
                _quantity = quantity;   
        }

        public override string ToString() {
            return String.Format("{0} change quantity of product {1} to {2}", this.Revision, this.ProductId, this.Quantity);
        }

        public long ProductId { get { return _productId; } }
        public int Quantity { get { return _quantity; } }
    }
}

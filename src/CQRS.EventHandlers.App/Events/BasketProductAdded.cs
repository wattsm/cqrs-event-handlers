using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Events {
    public class BasketProductAdded : BaseEvent {

        private readonly long _productId;
        private readonly int _quantity;

        public BasketProductAdded(Revision revision, long productId, int quantity)
            : base(revision) {

                _productId = productId;
                _quantity = quantity;
        }

        public override string ToString() {
            return String.Format("{0} add {1} of product {2}", this.Revision, this.Quantity, this.ProductId);
        }

        public long ProductId { get { return _productId; } }
        public int Quantity { get { return _quantity; } }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Events {
    public class BasketProductRemoved : BaseEvent {

        private readonly long _productId;

        public BasketProductRemoved(Revision revision, long productId)
            : base(revision) {

                _productId = productId;
        }

        public override string ToString() {
            return String.Format("{0} remove product {1}", this.Revision, this.ProductId);
        }

        public long ProductId { get { return _productId; } }
    }
}

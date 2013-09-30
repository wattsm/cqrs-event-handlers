using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Model {
    public struct BasketProduct {

        private readonly Product _product;
        private int _quantity;

        public BasketProduct(Product product, int quantity) {
            _product = product;
            _quantity = quantity;
        }

        public Product Product { get { return _product; } }

        public int Quantity {
            get { return _quantity; }
            set { _quantity = value; }
        }
    }
}

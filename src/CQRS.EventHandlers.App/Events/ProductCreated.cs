using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Events {
    public class ProductCreated : BaseEvent {

        private readonly string _name;
        private readonly decimal _price;

        public ProductCreated(long id, string name, decimal price)
            : base(id) {

                _name = name;
                _price = price;
        }

        public override string ToString() {
            return String.Format("{0} create product", this.Revision);
        }

        public string Name { get { return _name; } }
        public decimal Price { get { return _price; } }
    }
}

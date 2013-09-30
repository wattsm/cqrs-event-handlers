using CQRS.EventHandlers.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQRS.EventHandlers.Model {
    public class Product : BaseEntity {

        private readonly decimal _price;
        private readonly string _name;

        private Product(Events.ProductCreated @event)
            : base(@event) {

            _price = @event.Price;
            _name = @event.Name;
        }

        public static Product BasedOn(ProductCreated @event) {
            return new Product(@event);
        }

        public string Name { get { return _name; } }
        public decimal Price { get { return _price; } }        
    }
}

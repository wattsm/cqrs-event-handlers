using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CQRS.EventHandlers.Handlers;
using CQRS.EventHandlers.Model;
using CQRS.EventHandlers.Events;
using System.Threading;

namespace CQRS.EventHandlers {
    public static class App {

        public static void Main(string[] args) {
            
            var events = App.GenerateEvents(100, 25, 20);
            var versions = App.GetBasketVersions(events);

            var products = new InMemoryRepository<Product>();
            var baskets = new InMemoryRepository<Basket>();

            using(var productHandler = ProductEventHandler.Build(products)) {
                using(var basketHandler = BasketEventHandler.Build(baskets, products)) {

                    //Setup the bus
                    var bus = new Bus();
                    bus.Bind<Events.ProductCreated>(productHandler);
                    bus.Bind<Events.BasketCreated>(basketHandler);
                    bus.Bind<Events.BasketProductAdded>(basketHandler);
                    bus.Bind<Events.BasketProductQuantityChanged>(basketHandler);
                    bus.Bind<Events.BasketProductRemoved>(basketHandler);

                    //Setup a flag which we can use to wait until all the events have been processed
                    var flag = new ManualResetEvent(false);
                    var count = versions.Count();

                    foreach(var version in versions) {
                        baskets.Subscribe(version, () => {
                            if(Interlocked.Decrement(ref count) == 0) {
                                flag.Set();
                            }
                        });
                    }

                    var start = DateTime.Now;

                    //Publish messages
                    Parallel.ForEach(
                        events, 
                        @event => bus.Publish(@event)
                    );

                    //Wait until all events are processed
                    flag.WaitOne();

                    var end = DateTime.Now;
                                        
                    App.WriteTiming(events.Count(), start, end);
                }
            }

            Console.ReadLine();
        }

        private static IEnumerable<IEvent> GenerateEvents(int productCount, int basketCount, int maxEventsPerBasket) {

            var events = new List<IEvent>();
            var random = new Random(Environment.TickCount);
            var products = new List<long>();

            for(var productId = 1; productId <= productCount; productId++) {

                var price = random.Next(1, 11) * 5;
                var @event = new Events.ProductCreated(productId, String.Format("Product {0}", productId), price);

                events.Add(@event);
                products.Add(productId);
            }

            for(var basketId = 1; basketId <= basketCount; basketId++) {

                var eventCount = random.Next(1, maxEventsPerBasket + 1);
                var contents = new List<long>();

                for(var eventNo = 1; eventNo <= eventCount; eventNo++) {
                    if(eventNo == 1) {

                        events.Add(
                            new BasketCreated(basketId, String.Format("Basket {0}", basketId))
                        );

                    } else {

                        var revision = Revision.Update(basketId, eventNo);

                        Func<IEvent> add = () => {

                            var available = products.Where(p => !contents.Contains(p));
                            var productId = available.ElementAt(random.Next(0, available.Count()));

                            contents.Add(productId);

                            return new BasketProductAdded(
                                revision,
                                productId,
                                random.Next(1, 6)
                            );
                        };

                        Func<IEvent> change = () => {

                            return new BasketProductQuantityChanged(
                                revision,
                                contents.ElementAt(random.Next(0, contents.Count)),
                                random.Next(1, 6)
                            );

                        };

                        Func<IEvent> remove = () => {

                            var productId = contents.ElementAt(random.Next(0, contents.Count));
                            contents.Remove(productId);

                            return new BasketProductRemoved(
                                revision,
                                productId
                            );

                        };

                        IEvent @event;

                        if(contents.Count() == 0) {
                            @event = add();
                        } else if(contents.Count() == productCount) {
                            if(random.Next(1, 4) == 1) {
                                @event = remove();
                            } else {
                                @event = change();
                            }
                        } else {

                            var option = random.Next(1, 11);

                            if(option < 7) {
                                @event = add();
                            } else if(option < 9) {
                                @event = change();
                            } else {
                                @event = remove();
                            }
                        }

                        events.Add(@event);
                    }
                }
            }

            //Shuffle messages to they arrive out of order
            return events.OrderBy(_ => Guid.NewGuid());
        }

        private static IEnumerable<Revision> GetBasketVersions(IEnumerable<IEvent> events) {

            var excluded = new Type[] { typeof(ProductCreated) };

            return events
                    .Where(@event => !excluded.Contains(@event.GetType()))
                    .GroupBy(@event => @event.Revision.Id)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Max(@event => @event.Revision.Version)
                    )
                    .Select(item => Revision.Update(item.Key, item.Value));
        }

        private static void WriteState(int eventCount, IEnumerable<Revision> versions, IEnumerable<Basket> baskets) {

            Console.WriteLine("Processed {0} events, producing {1} baskets:", eventCount, versions.Count());
            Console.WriteLine("  ID   | Version | Expected | # Products (Nulls)");

            foreach(var basket in baskets) {

                var expected = versions.First(r => r.Id == basket.Revision.Id).Version;
                var actual = basket.Revision.Version;
                var productCount = basket.Products.Count();
                var nullCount = basket.Products.Where(p => p.Product == null).Count();

                Console.Write("  ");
                Console.Write(basket.Revision.Id.ToString().PadRight(5));

                Console.Write("| ");
                Console.Write(actual.ToString().PadRight(8));

                Console.Write("| ");
                Console.Write(expected.ToString().PadRight(9));

                Console.Write("| ");
                Console.Write("{0} ({1})", productCount, nullCount);

                Console.WriteLine();
            }
        }

        private static void WriteTiming(int eventCount, DateTime start, DateTime end) {

            var ms = end.Subtract(start).TotalMilliseconds;
            var msPerEvent = ms / eventCount;

            Console.WriteLine("{0} events, {1:0.00}ms in total, {2:0.00}ms per event", eventCount, ms, msPerEvent);

        }
    }
}

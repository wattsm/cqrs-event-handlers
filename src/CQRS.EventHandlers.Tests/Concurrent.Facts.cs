using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using System.Threading;
using Xunit.Extensions;

namespace CQRS.EventHandlers.Tests {
    public abstract class Concurrent_Decorator_HandleEvent_Facts {

        protected abstract IEventHandler<FakeEntity> Decorate(IEventHandler<FakeEntity> handler);

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void events_are_handled_by_wrapped_handler_on_a_single_thread(int eventCount) {

            var threadIds = new List<int>();
            var flag = new ManualResetEvent(false);

            var wrapped = new Mock<IEventHandler<FakeEntity>>();

            wrapped
                .Setup(x => x.HandleEvent(It.IsAny<IEvent>()))
                .Callback(() => {

                    threadIds.Add(Thread.CurrentThread.ManagedThreadId);

                    if(threadIds.Count == eventCount) {
                        flag.Set();
                    }
                });

            var concurrent = this.Decorate(wrapped.Object);
            var tasks = new List<Task>();

            for(var i = 0; i < eventCount; i++) {
                tasks.Add(Task.Run(() => {

                    var @event = new Mock<IEvent>();
                    concurrent.HandleEvent(@event.Object);

                }));
            }

            //Wait for all events to be added to the queue; this causes exceptions to be re-thrown which is handy
            Task.WaitAll(tasks.ToArray());

            //Wait for all events on the queue to be processed
            flag.WaitOne(); 

            Assert.Equal(1, threadIds.Distinct().Count());
        }

    }
}

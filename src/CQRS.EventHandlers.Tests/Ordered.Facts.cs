using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace CQRS.EventHandlers.Tests {
    public abstract class Ordered_Decorator_HandleEvent_Facts {

        protected abstract IEventHandler<FakeEntity> Decorate(IEventHandler<FakeEntity> handler);

        private IEvent CreateEvent(long id, long version) {

            var @event = new Mock<IEvent>();
            @event.Setup(x => x.Revision).Returns(Revision.Update(id, version));

            return @event.Object;
        }

        private Mock<IEventHandler<FakeEntity>> CreateHandler(long id, long version) {

            var repository = new Mock<IRepository<FakeEntity>>();
            repository.Setup(x => x.GetVersion(id)).Returns(version);

            var handler = new Mock<IEventHandler<FakeEntity>>();
            handler.Setup(x => x.Repository).Returns(repository.Object);

            return handler;
        }

        [Fact]
        public void event_with_expected_revision_is_applied() {

            var wrapped = this.CreateHandler(1L, 1L);
            var @event = this.CreateEvent(1L, 2L);

            var ordered = this.Decorate(wrapped.Object);
            ordered.HandleEvent(@event);

            wrapped.Verify(x => x.HandleEvent(@event), Times.Once());
        }

        [Fact]
        public void event_with_unexpected_revision_is_not_applied() {

            var wrapped = this.CreateHandler(1L, 1L);
            var @event = this.CreateEvent(1L, 3L);

            var ordered = this.Decorate(wrapped.Object);
            ordered.HandleEvent(@event);

            wrapped.Verify(x => x.HandleEvent(It.IsAny<IEvent>()), Times.Never());
        }

        [Fact]
        public void backlog_of_contiguous_successors_is_applied_in_ascending_version_order_after_event_is_applied() {

            var applied = new List<long>();

            var wrapped = this.CreateHandler(1L, 1L);
            wrapped.Setup(x => x.HandleEvent(It.IsAny<IEvent>())).Callback<IEvent>(@event => applied.Add(@event.Revision.Version));

            var events = new IEvent[] { this.CreateEvent(1L, 4L), this.CreateEvent(1L, 3L), this.CreateEvent(1L, 2L) };

            var ordered = this.Decorate(wrapped.Object);

            foreach(var @event in events) {
                ordered.HandleEvent(@event);
            }

            Assert.Equal(new long[] { 2L, 3L, 4L }, applied.ToArray());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace CQRS.EventHandlers.Tests {
    public abstract class Dependencies_Decorator_HandleEvent_Facts {

        protected abstract IEventHandler<FakeEntity> Decorate(IEventHandler<FakeEntity> handler, IVersionProvider versions, Func<FakeEvent, Revision> check);

        private IEventHandler<FakeEntity> Decorate(IEventHandler<FakeEntity> handler) {
            return this.Decorate(handler, null, null);
        }        

        private Mock<IVersionProvider> CreateProvider(long id, long version) {

            var provider = new Mock<IVersionProvider>();
            provider.Setup(x => x.GetVersion(id)).Returns(version);

            return provider;
        }

        private Mock<IEventHandler<FakeEntity>> CreateHandler() {
            return new Mock<IEventHandler<FakeEntity>>();
        }

        [Fact]
        public void event_with_no_dependencies_is_applied() {

            var handler = this.CreateHandler();
            var @event = new FakeEvent();
            var versions = this.CreateProvider(1L, 1L);

            var depends = this.Decorate(handler.Object, versions.Object, _ => Revision.Empty);
            depends.HandleEvent(@event);

            handler.Verify(x => x.HandleEvent(@event), Times.Once());
        }

        [Fact]
        public void event_with_satisfied_dependency_is_applied() {

            var handler = this.CreateHandler();
            var @event = new FakeEvent();
            var versions = this.CreateProvider(1L, 1L);

            var depends = this.Decorate(handler.Object, versions.Object, _ => Revision.Create(1L));
            depends.HandleEvent(@event);

            handler.Verify(x => x.HandleEvent(@event), Times.Once());
        }

        [Fact]
        public void event_with_unsatisfied_dependency_is_not_applied() {

            var handler = this.CreateHandler();
            var @event = new FakeEvent();
            var versions = this.CreateProvider(1L, 1L);

            var depends = this.Decorate(handler.Object, versions.Object, _ => Revision.Update(1L, 2L));
            depends.HandleEvent(@event);

            handler.Verify(x => x.HandleEvent(@event), Times.Never());
        }

        [Fact]
        public void event_with_unsatisfied_dependency_causes_subscription_for_desired_revision_to_be_created() {

            var handler = this.CreateHandler();
            var @event = new FakeEvent();
            var versions = this.CreateProvider(1L, 1L);
            var required = Revision.Update(1L, 2L);

            var depends = this.Decorate(handler.Object, versions.Object, _ => required);
            depends.HandleEvent(@event);

            versions.Verify(x => x.Subscribe(required, It.IsAny<Action>()), Times.Once());
        }

        [Fact]
        public void event_with_unsatisfied_dependency_is_reapplied_when_dependency_is_satisfied() {

            Action callback = null;

            var handler = this.CreateHandler();
            var @event = new FakeEvent();            
            var required = Revision.Update(1L, 2L);

            var versions = this.CreateProvider(1L, 1L);
            versions.Setup(x => x.Subscribe(It.IsAny<Revision>(), It.IsAny<Action>())).Callback<Revision, Action>((_, action) => callback = action);

            var depends = this.Decorate(handler.Object, versions.Object, _ => required);
            depends.HandleEvent(@event);

            versions.Setup(x => x.GetVersion(1L)).Returns(2L);
            callback();

            handler.Verify(x => x.HandleEvent(@event), Times.Once());
        }        
    }
}

using System.Collections.Generic;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Velo.Collections;
using Velo.ECS.Actors.Context;
using Velo.ECS.Actors.Filters;
using Velo.ECS.Components;
using Velo.TestsModels.ECS;
using Xunit;

namespace Velo.Tests.ECS.Actors
{
    public class ActorFilter1Should : ECSTestClass
    {
        private readonly TestActor _actor;
        private readonly Mock<IActorContext> _actorContext;
        private readonly IActorFilter<TestComponent1> _actorFilter;

        public ActorFilter1Should()
        {
            var components = new IComponent[] {new TestComponent1()};
            _actor = new TestActor(1, components);
            _actorContext = new Mock<IActorContext>();
            _actorFilter = new ActorFilter<TestComponent1>(_actorContext.Object);

            InjectComponentsArray(components);
        }

        [Fact]
        public void AddActor()
        {
            RaiseActorAdded(_actorContext, _actor);

            _actorFilter.Contains(_actor.Id).Should().BeTrue();
        }

        [Theory]
        [AutoData]
        public void Enumerable(int length)
        {
            Fixture
                .CreateMany<TestActor>(length)
                .Foreach(actor => RaiseActorAdded(_actorContext, actor));

            var exists = new HashSet<int>();

            foreach (var actor in _actorFilter)
            {
                exists.Add(actor.Id).Should().BeTrue();

                actor.Component1
                    .Should().NotBeNull().And
                    .BeOfType<TestComponent1>();
            }

            exists.Count.Should().Be(length);
        }

        [Fact]
        public void EnumerableWhere()
        {
            foreach (var actor in Fixture.CreateMany<TestActor>())
            {
                RaiseActorAdded(_actorContext, actor);

                _actorFilter
                    .Where((a, id) => a.Id == id, actor.Id)
                    .Should().ContainSingle(a => a.Id == actor.Id);
            }
        }

        [Theory]
        [AutoData]
        public void HasLength(int length)
        {
            for (var i = 0; i < length; i++)
            {
                RaiseActorAdded(_actorContext, _actor);
            }

            _actorFilter.Length.Should().Be(length);
        }

        [Fact]
        public void NotAddActor()
        {
            var actorId = _actor.Id + 1;
            RaiseActorAdded(_actorContext, BuildActor(actorId));

            _actorFilter.Contains(actorId).Should().BeFalse();
        }

        [Fact]
        public void NotRemoveActor()
        {
            RaiseActorAdded(_actorContext, _actor);

            var actorId = _actor.Id + 1;
            RaiseActorAdded(_actorContext, BuildActor(actorId));

            _actorFilter.Contains(_actor.Id).Should().BeTrue();
        }

        [Fact]
        public void RaiseAdded()
        {
            using var actorFilter = _actorFilter.Monitor();

            RaiseActorAdded(_actorContext, _actor);

            actorFilter.Should().Raise(nameof(IActorFilter<TestComponent1>.Added));
        }

        [Fact]
        public void RaiseRemoved()
        {
            using var actorFilter = _actorFilter.Monitor();

            RaiseActorAdded(_actorContext, _actor);
            RaiseActorRemoved(_actorContext, _actor);

            actorFilter.Should().Raise(nameof(IActorFilter<TestComponent2>.Removed));
        }

        [Fact]
        public void RemoveActor()
        {
            RaiseActorAdded(_actorContext, _actor);
            RaiseActorRemoved(_actorContext, _actor);

            _actorFilter.Contains(_actor.Id).Should().BeFalse();
        }

        [Fact]
        public void TryGetTrue()
        {
            RaiseActorAdded(_actorContext, _actor);

            _actorFilter.TryGet(_actor.Id, out var exists).Should().BeTrue();
            exists.Entity.Should().Be(_actor);
        }

        [Fact]
        public void TryGetFalse()
        {
            _actorFilter.TryGet(-_actor.Id, out _).Should().BeFalse();
        }
    }
}
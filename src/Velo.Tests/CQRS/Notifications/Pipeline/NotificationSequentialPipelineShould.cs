using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Velo.CQRS.Notifications;
using Velo.CQRS.Notifications.Pipeline;
using Velo.TestsModels.Emitting.Boos.Create;
using Xunit;

namespace Velo.Tests.CQRS.Notifications.Pipeline
{
    public class NotificationSequentialPipelineShould : CQRSTestClass
    {
        private readonly Notification _notification;

        private readonly INotificationPipeline<Notification> _pipeline;
        private readonly Mock<INotificationProcessor<Notification>>[] _processors;

        public NotificationSequentialPipelineShould()
        {
            _notification = new Notification();

            _processors = Many(() => MockNotificationProcessor(_notification));

            _pipeline = new NotificationSequentialPipeline<Notification>(_processors
                .Select(mock => mock.Object)
                .ToArray());
        }

        [Fact]
        public void DisposeAfterPublish()
        {
            _pipeline
                .Awaiting(pipeline => pipeline.Publish(_notification, CancellationToken))
                .Should().NotThrow();

            _pipeline
                .Invoking(pipeline => pipeline.Dispose())
                .Should().NotThrow();
        }
        
        [Fact]
        public void PublishNotification()
        {
            _pipeline
                .Awaiting(pipeline => pipeline.Publish(_notification, CancellationToken))
                .Should().NotThrow();

            EnsurePipelineExecuted();
        }

        [Theory, AutoData]
        public void PublishNotificationManyTimes(int count)
        {
            count = EnsureValid(count);

            for (var i = 0; i < count; i++)
            {
                _pipeline
                    .Awaiting(pipeline => pipeline.Publish(_notification, CancellationToken))
                    .Should().NotThrow();
            }

            EnsurePipelineExecuted(count);
        }

        [Theory, AutoData]
        public void PublishNotificationManyTimesParallel(int count)
        {
            count = EnsureValid(count);

            Parallel.For(0, count, _ =>
            {
                // ReSharper disable once AccessToDisposedClosure
                _pipeline
                    .Awaiting(pipeline => pipeline.Publish(_notification, CancellationToken))
                    .Should().NotThrow();
            });

            EnsurePipelineExecuted(count);
        }
        
        [Fact]
        public void PublishByInterface()
        {
            var pipeline = (INotificationPipeline) _pipeline;

            pipeline
                .Awaiting(p => p.Publish(_notification, CancellationToken))
                .Should().NotThrow();

            EnsurePipelineExecuted();
        }

        [Fact]
        public void StopPropagation()
        {
            _notification.StopPropagation = true;
            
            _pipeline
                .Awaiting(pipeline => pipeline.Publish(_notification, CancellationToken))
                .Should().NotThrow();

            foreach (var processor in _processors)
            {
                processor
                    .Verify(p => p
                        .Process(_notification, CancellationToken),Times.Never);
            }
        }
        
        [Fact]
        public void ThrowIfDisposed()
        {
            _pipeline.Dispose();

            _pipeline
                .Awaiting(pipeline => pipeline.Publish(_notification, CancellationToken))
                .Should().Throw<ObjectDisposedException>();
        }
        
        private void EnsurePipelineExecuted(int count = 1)
        {
            foreach (var processor in _processors)
            {
                processor
                    .Verify(p => p
                        .Process(_notification, CancellationToken), Times.Exactly(count));
            }
        }
    }
}
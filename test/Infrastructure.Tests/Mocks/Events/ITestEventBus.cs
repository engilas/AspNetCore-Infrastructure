using Infrastructure.Events;

namespace Infrastructure.Tests.Mocks.Events
{
    public interface ITestEventBus : IEventBus<TestEventType, TestEventData>
    {
    }
}

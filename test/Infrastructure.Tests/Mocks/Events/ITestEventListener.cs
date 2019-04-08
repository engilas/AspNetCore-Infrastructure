using Infrastructure.Events;

namespace Infrastructure.Tests.Mocks.Events
{
    public interface ITestEventListener : IEventListener<ITestEventBus, TestEventType, TestEventData>
    {
    }
}

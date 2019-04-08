namespace Infrastructure.Events
{
    public interface IEventBus<in TEventType, out TEventData>
    {
        void SubscribeOnEvent<T>(TEventType eventType) where T : IEventListener;
    }
}

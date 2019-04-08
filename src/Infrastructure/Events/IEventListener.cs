using System.Threading.Tasks;

namespace Infrastructure.Events
{
    public interface IEventListener
    {

    }

    public interface IEventListener<TBus, out TEventType, in TEventData> : IEventListener
    where TBus: IEventBus<TEventType, TEventData>
    {
        TEventType ListenType { get; }
        Task ProcessEvent(TEventData eventData);
    }
}

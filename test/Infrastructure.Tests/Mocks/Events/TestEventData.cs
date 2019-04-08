namespace Infrastructure.Tests.Mocks.Events
{
    public class TestEventData
    {
        public int UserId { get; set; }
        public TestEventType EventType { get; set; }
        public string Data { get; set; }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using NSubstitute;

namespace Infrastructure.Tests.Mocks
{
    public class LogEvent
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return $"{Level} {Message} {Exception.Message}";
        }
    }

    public class Logger_Mock<T>
    {
        private readonly ConcurrentBag<LogEvent> _accumulatedLogs = new ConcurrentBag<LogEvent>();
        private readonly ILogger<T> _loggerMock;

        public Logger_Mock()
        {
            _loggerMock = Substitute.For<ILogger<T>>();

            _loggerMock.When(x => x.Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(),
                    Arg.Any<Exception>(), Arg.Any<Func<object, Exception, string>>()))
                .Do(x =>
                {
                    var level = x.ArgAt<LogLevel>(0);
                    var format = x.ArgAt<FormattedLogValues>(2);
                    var ex = x.ArgAt<Exception>(3);

                    var logEvent = new LogEvent
                    {
                        Level = level,
                        Message = format.ToString(),
                        Exception = ex
                    };

                    _accumulatedLogs.Add(logEvent);
                    OnNewEvent?.Invoke(logEvent);
                });
        }

        public List<LogEvent> AccumulatedLogs => _accumulatedLogs.ToList();

        public event Action<LogEvent> OnNewEvent;

        public ILogger<T> Get()
        {
            return _loggerMock;
        }
    }
}
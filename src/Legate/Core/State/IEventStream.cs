using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Legate.Core.State
{
    public interface IEventStream<T> : IDisposable
    {
        IAsyncEnumerable<T> ReadEventsAsync(CancellationToken cancellationToken = default);
        ValueTask WriteEventAsync(T data);
    }

    public abstract class BaseEventStream<T> : IEventStream<T>
    {
        private readonly Channel<T> _channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });

        public IAsyncEnumerable<T> ReadEventsAsync(CancellationToken cancellationToken = default)
        {
            return _channel.Reader.ReadAllAsync(cancellationToken);
        }

        public async ValueTask WriteEventAsync(T data)
        {
            await _channel.Writer.WriteAsync(data);
        }

        public void Dispose()
        {
            _channel.Writer.Complete();
        }
    }
}
using Common.Caching.Interfaces;

namespace Common.Caching
{
    public abstract class MessageBus : IMessageBus
    {
        protected MessageBus(string exchangeName, string exchangeType)
        {
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            ExchangeType = exchangeType ?? throw new ArgumentNullException(nameof(exchangeType));
        }

        public string ExchangeName { get; }
        public string ExchangeType { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool dispose);
    }
}
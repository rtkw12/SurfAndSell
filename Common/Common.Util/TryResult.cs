namespace Common.Util
{
    public class TryResult
    {
        protected TryResult()
        {
            Success = true;
            ErrorMessage = string.Empty;
        }

        protected TryResult(string message)
        {
            Success = false;
            ErrorMessage = message ?? throw new ArgumentNullException(nameof(message));
        }

        public bool Success { get; }
        public string ErrorMessage { get; }

        public static TryResult Succeed()
        {
            return new TryResult();
        }

        public static TryResult Fail(string error)
        {
            return new TryResult(error);
        }
    }

    public class TryResult<T> : TryResult
    {
        private TryResult(T value)
        {
            Value = value;
        }

        private TryResult(string error) : base(error)
        {
            Value = default(T)!;
        }

        public T Value { get; }

        public static TryResult<T> Succeed(T value)
        {
            return new TryResult<T>(value);
        }

        public new static TryResult<T> Fail(string error)
        {
            return new TryResult<T>(error);
        }
    }
}
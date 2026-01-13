namespace Insurance.Application.Common.Exceptions;

    public abstract class AppException : Exception
    { 
        protected AppException(string message) : base(message) { }

        public abstract int StatusCode { get; }
        public virtual string ErrorCode => GetType().Name;
        public virtual object? Details => null;
    }

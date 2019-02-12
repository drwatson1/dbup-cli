namespace DbUp.Cli
{
    public class Error
    {
#pragma warning disable RECS0154 // Parameter is never used
        public Error(string message) => Message = message;
#pragma warning restore RECS0154 // Parameter is never used

        public string Message { get; private set; }

        public static Error Create(string message) => new Error(message);
    }
}

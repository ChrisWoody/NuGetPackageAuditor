namespace NuGetPackageAuditor
{
    public class ResultWrapper<T>
    {
        public T Result { get; }

        public bool IsSuccess { get; }
        public string Error { get; }

        private ResultWrapper(T theResult, bool isSuccess, string error)
        {
            Result = theResult;
            IsSuccess = isSuccess;
            Error = error;
        }

        public static ResultWrapper<T> Success(T theResult)
        {
            return new ResultWrapper<T>(theResult, true, null);
        }

        public static ResultWrapper<T> Failure(string error)
        {
            return new ResultWrapper<T>(default, false, error);
        }
    }
}
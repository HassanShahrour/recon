namespace Reconova.BusinessLogic.Exceptions
{
    /// <summary>
    /// Represents the result of an operation, encapsulating either a value or an error.
    /// Provides methods to indicate success or failure.
    /// </summary>
    /// <typeparam name="T">The type of the value returned in case of success.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class with a successful result.
        /// </summary>
        /// <param name="value">The value of the successful result.</param>
        private Result(T value)
        {
            Value = value;
            Error = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{T}"/> class with an error message.
        /// </summary>
        /// <param name="error">The error message describing the failure.</param>
        /// <exception cref="ArgumentException">Thrown if the provided error message is null or empty.</exception>
        private Result(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                throw new ArgumentException("Error message must be provided", nameof(error));

            Error = error;
            Value = default;
        }

        /// <summary>
        /// Gets the value of the result if the operation was successful.
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// Gets the error message if the operation failed.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// Indicates whether the result was successful.
        /// </summary>
        public bool IsSuccess => Error == null;

        /// <summary>
        /// Creates a successful result with the specified value.
        /// </summary>
        /// <param name="value">The value of the successful result.</param>
        /// <returns>A <see cref="Result{T}"/> representing a successful operation.</returns>
        public static Result<T> Success(T value) => new Result<T>(value);

        /// <summary>
        /// Creates a failed result with the specified error message.
        /// </summary>
        /// <param name="error">The error message describing the failure.</param>
        /// <returns>A <see cref="Result{T}"/> representing a failed operation.</returns>
        public static Result<T> Failure(string error) => new Result<T>(error);
    }
}

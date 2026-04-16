using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fourteen.Domain.Common
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && !string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Success result cannot have an error message");

            if (!isSuccess && string.IsNullOrEmpty(error))
                throw new InvalidOperationException("Failure result must have an error message");

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, string.Empty);
        public static Result Failure(string error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
        public static Result<T> Failure<T>(string error) => new(default!, false, error);
    }

    public class Result<T> : Result
    {
        private readonly T _value = default!;

        public T Value
        {
            get
            {
                if (IsFailure)
                    throw new InvalidOperationException(
                        $"Cannot access Value of a failed result. Error: {Error}");

                return _value;
            }
        }

        protected internal Result(T value, bool isSuccess, string error)
            : base(isSuccess, error)
        {
            if (isSuccess && value is null)
                throw new InvalidOperationException("Success result cannot have a null value");

            _value = value;
        }

        public static implicit operator Result<T>(T value) => Success(value);

        // Add helper method to safely get value
        public bool TryGetValue([NotNullWhen(true)] out T? value)
        {
            value = IsSuccess ? _value : default;
            return IsSuccess;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using AppFactory.Framework.Domain.Infrastructure;

namespace AppFactory.Framework.Domain.Commands
{
    public class OperationResult
    {
        internal const string DefaultPropertyName = "N/A";

        public static OperationResult Ok => new OperationResult();

        public static OperationResult Failed(string message)
        {
            return new OperationResult(message);
        }

        private readonly List<OperationResultError> _errors = new List<OperationResultError>();

        public OperationResult()
        { }

        public OperationResult(string errorText)
            : this(DefaultPropertyName, errorText)
        { }

        public OperationResult(IEnumerable<OperationResultError> failures)
        {
            _errors = failures.ToList();
        }

        public OperationResult(OperationResult businnessResult)
            : this(businnessResult.Errors)
        { }

        public OperationResult(string propertyName, string errorText, object attemptedValue = null)
        {
            if (!string.IsNullOrWhiteSpace(errorText))
            {
                AddFailure(propertyName, errorText, attemptedValue);
            }
        }

        public IList<OperationResultError> Errors => _errors;

        public bool IsValid => _errors.Count == 0;

        public void AddFailure(string propertyName, string error, object attemptedValue = null)
        {
            AddFailure(new OperationResultError(propertyName, error, attemptedValue));
        }

        public void AddFailure(OperationResultError validationFailure)
        {
            Check.NotNull(validationFailure, "validationFailure");
            _errors.Add(validationFailure);
        }

        public void Merge(OperationResult validationResult)
        {
            Check.NotNull(validationResult, "validationResult");
            Merge(validationResult.Errors);
        }

        public void Merge(IEnumerable<OperationResultError> validationResult)
        {
            Check.NotNull(validationResult, "validationResult");
            foreach (var error in validationResult)
            {
                _errors.Add(error);
            }
        }
    }

    public class OperationResultError
    {
        public OperationResultError(string propertyName, string error)
        {
            PropertyName = propertyName;
            ErrorMessage = error;
        }

        public OperationResultError(string propertyName, string error, object data)
            : this(propertyName, error)
        {
            Data = data;
        }

        public string PropertyName { get; private set; }
        public string ErrorMessage { get; private set; }
        public object Data { get; private set; }
    }


    public class OperationResult<TValue> : OperationResult
    {
        public OperationResult()
        { }

        public OperationResult(IEnumerable<OperationResultError> failures)
            : base(failures)
        { }

        public OperationResult(OperationResult businnessResult)
            : base(businnessResult.Errors)
        { }

        public OperationResult(string propertyName, string error, object attemptedValue = null)
            : base(propertyName, error, attemptedValue)
        { }

        public OperationResult(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; set; }

        public new static OperationResult<TValue> Ok
        {
            get { return new OperationResult<TValue>(); }
        }

        public new static OperationResult<TValue> Failed(string message)
        {
            return new OperationResult<TValue>(DefaultPropertyName, message);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using AppFactory.Framework.Domain.Infrastructure;

namespace AppFactory.Framework.Domain.Commands
{
    public class CommandResult
    {
        internal const string DefaultPropertyName = "N/A";

        public static CommandResult Ok => new CommandResult();

        public static CommandResult Failed(string message)
        {
            return new CommandResult(message);
        }

        private readonly List<OperationResultError> _errors = new List<OperationResultError>();

        public CommandResult()
        { }

        public CommandResult(string errorText)
            : this(DefaultPropertyName, errorText)
        { }

        public CommandResult(IEnumerable<OperationResultError> failures)
        {
            _errors = failures.ToList();
        }

        public CommandResult(CommandResult result)
            : this(result.Errors)
        { }

        public CommandResult(string propertyName, string errorText, object attemptedValue = null)
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

        public void Merge(CommandResult validationResult)
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


    public class CommandResult<TValue> : CommandResult
    {
        public CommandResult()
        { }

        public CommandResult(IEnumerable<OperationResultError> failures)
            : base(failures)
        { }

        public CommandResult(CommandResult result)
            : base(result.Errors)
        { }

        public CommandResult(string propertyName, string error, object attemptedValue = null)
            : base(propertyName, error, attemptedValue)
        { }

        public CommandResult(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; set; }

        public new static CommandResult<TValue> Ok
        {
            get { return new CommandResult<TValue>(); }
        }

        public new static CommandResult<TValue> Failed(string message)
        {
            return new CommandResult<TValue>(DefaultPropertyName, message);
        }
    }
}

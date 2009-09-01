using System.Text;
using NHibernate.Validator.Engine;

namespace Rhino.Security.Exceptions
{
	/// <summary>
	/// Thrown when validation occurs.
	/// </summary>
	[global::System.Serializable]
	public class ValidationException : System.Exception
	{
        private readonly IConstraintValidatorContext errorSummary;

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="errorSummary">The error summary.</param>
        public ValidationException(ConstraintValidatorContext errorSummary)
            : base(GenerateMessage(errorSummary))
		{
			this.errorSummary = errorSummary;
		}

        private static string GenerateMessage(ConstraintValidatorContext summary)
		{
			var sb = new StringBuilder();
			sb.AppendLine("Validation errors:");
			foreach (var invalidProperty in summary.InvalidMessages)
			{
			    sb.Append(invalidProperty.PropertyName)
                    .Append(" ")
                    .AppendLine(invalidProperty.Message);
			}
            return sb.ToString();
		}

		/// <summary>
		/// Gets the error summary with details about the validation error
		/// </summary>
		/// <value>The error summary.</value>
        public IConstraintValidatorContext ErrorSummary
		{
			get { return errorSummary; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ValidationException"/> class.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
		/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
		protected ValidationException(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
			: base(info, context) { }
	}
}
using Castle.Components.Validator;
using Rhino.Security.Impl.Validatiors;
using Rhino.Security.Model;

namespace Rhino.Security.Model
{
	/// <summary>
	/// An entity with a name
	/// </summary>
	public class NamedEntity<T> : EqualityAndHashCodeProvider<T> 
		where T : NamedEntity<T>
	{
		private string name;

		/// <summary>
		/// Gets or sets the name of this entity.
		/// </summary>
		/// <value>The name.</value>
		/// <remarks>
		/// The name can be set only on creation, and is not changed
		/// afterward.
		/// </remarks>
		[ValidateNonEmpty, ValidateLength(0, 255), ValidateIsUniqueInSameTransaction]
		public virtual string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return GetType().Name + ": " + Name;
		}
	}
}
namespace Rhino.Security
{
    using Castle.ActiveRecord;
    using Castle.Components.Validator;

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
        [Property(NotNull = true, Length = 255, Update = false, Unique = true)]
        [ValidateIsUnique, ValidateNonEmpty, ValidateLength(0, 255)]
        public virtual string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
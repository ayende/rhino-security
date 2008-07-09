using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Mapping;

namespace Rhino.Security.Impl.MappingRewriting
{
	/// <summary>
	/// Modify the NH configuration to set the correct schema
	/// </summary>
	public class SchemaChanger
	{
		private readonly Configuration cfg;
		private readonly SecurityTableStructure tableStructure;

		/// <summary>
		/// Initializes a new instance of the <see cref="SchemaChanger"/> class.
		/// </summary>
		/// <param name="cfg">The NHibernte configuration.</param>
		/// <param name="tableStructure">The table structure.</param>
		public SchemaChanger(Configuration cfg, SecurityTableStructure tableStructure)
		{
			this.cfg = cfg;
			this.tableStructure = tableStructure;
		}

		/// <summary>
		/// Modify the configuration so it would set the schema or prefix based on the selected
		/// table structure
		/// </summary>
		public void Change()
		{
			foreach (PersistentClass persistentClass in cfg.ClassMappings)
			{
				//ignore non Rhino Security entities
				if (persistentClass.MappedClass.Assembly != typeof(IUser).Assembly)
					continue;
				if (tableStructure == SecurityTableStructure.Schema)
					persistentClass.Table.Schema = "security";
				else if (persistentClass.Table.Name.StartsWith("security_") == false)
					persistentClass.Table.Name = "security_" + persistentClass.Table.Name;

				foreach (Property property in persistentClass.PropertyIterator)
				{
					Collection collection = property.Value as Collection;
					if (collection == null) 
						continue;
					if (tableStructure == SecurityTableStructure.Schema && collection.CollectionTable != null)
						collection.CollectionTable.Schema = "security";
					else if (tableStructure == SecurityTableStructure.Prefix && collection.Table.Name.StartsWith("security_") == false)
						collection.CollectionTable.Name = "security_" + collection.Table.Name;
				}
			}
		}
	}
}

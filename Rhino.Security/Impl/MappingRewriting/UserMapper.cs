using System;
using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping;
using NHibernate.Type;

namespace Rhino.Security.Impl.MappingRewriting
{
	/// <summary>
	/// Map <seealso cref="IUser"/> to the domain model implementation of the User entity.
	/// </summary>
	public class UserMapper
	{
		private readonly Configuration cfg;
		private readonly Type userType;
		private bool performedMapping = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="UserMapper"/> class.
		/// </summary>
		/// <param name="cfg">The NHibernate configuration.</param>
		/// <param name="userType">Type of the user.</param>
		public UserMapper(Configuration cfg, Type userType)
		{
			this.cfg = cfg;
			this.userType = userType;
		}

		/// <summary>
		/// Modify the configuration so it would set all the references to <see cref="IUser"/>
		/// to userEntityName.
		/// </summary>
		public void Map()
		{
			Dialect dialect = Dialect.GetDialect(cfg.Properties);
			Mappings mappings = cfg.CreateMappings(dialect);
			mappings.AddSecondPass(delegate { PerformMapping(); } );
		}

		private void PerformMapping()
		{
			if (performedMapping)
				return;
			performedMapping = true;
			
			PersistentClass classMapping = GetUserMapping();

			foreach (PersistentClass persistentClass in cfg.ClassMappings)
			{
				//ignore non Rhino Security entities
				if (persistentClass.MappedClass.Assembly != typeof(IUser).Assembly)
					continue;

				foreach (Property property in persistentClass.PropertyIterator)
				{
					HandleManyToOne(property, classMapping);

					HandleManyToManySet(property, persistentClass);
				}
				UpdateForeignKeyReferences(classMapping, persistentClass.Table);
			}
		}

		private PersistentClass GetUserMapping()
		{
			PersistentClass classMapping = cfg.GetClassMapping(userType);
			if (classMapping==null)
			{
				throw new InvalidOperationException("User must be a mapped type, but could not find the class: " + userType + " in NHibernate mapping.");
			}
			return classMapping;
		}

		private void UpdateForeignKeyReferences(PersistentClass classMapping, Table table)
		{
			foreach (ForeignKey key in table.ForeignKeyIterator)
			{
				if (key.ReferencedEntityName != typeof(IUser).FullName)
					continue;

				key.ReferencedEntityName = userType.FullName;
				key.ReferencedTable = classMapping.Table;
			}
		}

		private void HandleManyToManySet(Property property, PersistentClass classMapping)
		{
			Collection collection = property.Value as Collection;
			if (collection == null || collection.IsOneToMany ||
				collection.GenericArguments.Length != 1 ||
				collection.GenericArguments[0] != typeof(IUser))
				return;
			UpdateForeignKeyReferences(classMapping, collection.Element.Table);
			ManyToOne one = collection.Element as ManyToOne;
			if (one != null)
			{
				ManyToOne element = new ManyToOne(one.Table);
				element.IsLazy = one.IsLazy;
				element.ReferencedEntityName = userType.FullName;
				element.ReferencedPropertyName = one.ReferencedPropertyName;
				element.IsIgnoreNotFound = one.IsIgnoreNotFound;
				CopyColumns(one, element);
				collection.Element = element;
			}
		}

		private void HandleManyToOne(Property property, PersistentClass classMapping)
		{
			ManyToOne manyToOne = property.Value as ManyToOne;
			if (manyToOne == null || manyToOne.ReferencedEntityName != typeof(IUser).FullName)
				return;
			ManyToOne value = new ManyToOne(classMapping.Table);
			value.ReferencedEntityName = userType.FullName;
			CopyColumns(manyToOne, value);
			property.Value = value;
		}

		private void CopyColumns(ManyToOne src, ManyToOne dest)
		{
			foreach (ISelectable selectable in src.ColumnIterator)
			{
				Column col = selectable as Column;
				if (col != null)
				{
					dest.AddColumn(col);
				}
			}
		}
	}
}
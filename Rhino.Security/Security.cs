namespace Rhino.Security
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Castle.ActiveRecord;
    using Castle.ActiveRecord.Framework;
    using Castle.ActiveRecord.Framework.Internal;
    using Castle.Core.Logging;
    using Commons;

    /// <summary>
    /// This class allows to configure the security system
    /// </summary>
    public class Security
    {
        /// <summary>
        /// A flag that determains how the security tables should be treated.
        /// In a separate schema or using a naming convention.
        /// The default is to put them in a separate schema.
        /// </summary>
        public static bool UseSecuritySchema = true;

        /// <summary>
        /// Prepares to change all internal reference in the security system
        /// from IUser to the user implementation of the project
        /// </summary>
        public static void PrepareForActiveRecordInitialization<TUser>()
            where TUser : IUser
        {
            ActiveRecordStarter.ModelsCreated +=
                delegate(ActiveRecordModelCollection models, IConfigurationSource source)
                {
                    Type userType = typeof(TUser);
                    foreach (ActiveRecordModel model in models)
                    {
                        if (model.Type.Assembly != typeof(IUser).Assembly)
                            continue;
                        model.ActiveRecordAtt.Cache = CacheEnum.ReadWrite;
                        if (UseSecuritySchema)
                            model.ActiveRecordAtt.Schema = "security";
                        else
                            model.ActiveRecordAtt.Table = "security_" + model.ActiveRecordAtt.Table;
                        foreach (BelongsToModel belongsToModel in model.BelongsTo)
                        {
                            if (belongsToModel.Property.PropertyType == typeof(IUser))
                            {
                                belongsToModel.BelongsToAtt.Type = userType;
                            }
                        }
                        foreach (HasAndBelongsToManyModel hasAndBelongsToManyModel in model.HasAndBelongsToMany)
                        {
                            hasAndBelongsToManyModel.HasManyAtt.Cache = CacheEnum.ReadWrite;
                            
                            if (UseSecuritySchema)
                            {
                                hasAndBelongsToManyModel.HasManyAtt.Schema = "security";
                            }
                            else
                            {
                                hasAndBelongsToManyModel.HasManyAtt.Table = "security_" +
                                                                            hasAndBelongsToManyModel.HasManyAtt.Table;
                            }
                            if (hasAndBelongsToManyModel.HasManyAtt.MapType == typeof(IUser))
                            {
                                hasAndBelongsToManyModel.HasManyAtt.MapType = userType;
                            }
                        }
                        foreach (HasManyModel hasManyModel in model.HasMany)
                        {
                            hasManyModel.HasManyAtt.Cache = CacheEnum.ReadWrite;

                            if (hasManyModel.HasManyAtt.MapType == typeof(IUser))
                            {
                                hasManyModel.HasManyAtt.MapType = userType;
                            }
                        }
                    }
                };
        }

        /// <summary>
        /// Gets the logger for the security system.
        /// </summary>
        /// <value>The logger.</value>
        public static ILogger Logger
        {
            get
            {
                return IoC.TryResolve<ILogger>(new NullLogger());

            }
        }

        /// <summary>
        /// Extracts the key from the specified entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static Guid ExtractKey<TEntity>(TEntity entity)
            where TEntity : class
        {
            Guard.Against<ArgumentNullException>(entity == null, "Entity cannot be null");
            IEntityInformationExtractor<TEntity> extractor = IoC.Resolve<IEntityInformationExtractor<TEntity>>();
            return extractor.GetSecurityKeyFor(entity);
        }

		/// <summary>
		/// Gets a human readable description for the specified entity
		/// </summary>
		/// <typeparam name="TEntity">The type of the entity.</typeparam>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		 public static string GetDescription<TEntity>(TEntity entity) where TEntity : class
		{
            IEntityInformationExtractor<TEntity> extractor = IoC.Resolve<IEntityInformationExtractor<TEntity>>();
            return extractor.GetDescription(ExtractKey(entity));
		}

        /// <summary>
        /// Gets the security key property for the specified entity type
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetSecurityKeyProperty(Type entityType)
        {
            lock(GetSecurityKeyPropertyCache)
            {
                Func<string> func;
                if (GetSecurityKeyPropertyCache.TryGetValue(entityType, out func))
                    return func();
                func = (Func<string>)
                    Delegate.CreateDelegate(typeof (Func<string>), 
                        getSecurityKeyMethod.MakeGenericMethod(entityType));
                GetSecurityKeyPropertyCache[entityType] = func;
                return func();
            }
        }

        internal static string GetSecurityKeyPropertyInternal<TEntity>()
        {
            return IoC.Resolve<IEntityInformationExtractor<TEntity>>().SecurityKeyPropertyName;
        }

        private readonly static Dictionary<Type, Func<string>> GetSecurityKeyPropertyCache = new Dictionary<Type, Func<string>>();
        private readonly static MethodInfo getSecurityKeyMethod = typeof(Security).GetMethod("GetSecurityKeyPropertyInternal", BindingFlags.NonPublic | BindingFlags.Static);
    }
}
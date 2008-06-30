using System;
using Rhino.Security.Impl;
using Rhino.Security.Interfaces;
using Rhino.Security.Services;

namespace Rhino.Security
{
    using Castle.Components.Validator;
    using Castle.Core;
    using Castle.MicroKernel.Facilities;
    using Castle.MicroKernel.Registration;
    using Commons;

    /// <summary>
    /// Setup everything necessary for Rhino Security to function
    /// </summary>
    public class RhinoSecurityFacility : AbstractFacility
    {
    	private readonly SecurityTableStructure securityTableStructure;
    	private readonly Type userType;

		/// <summary>
		/// Initializes a new instance of the <see cref="RhinoSecurityFacility"/> class.
		/// </summary>
		/// <param name="userType">Type of the user.</param>
    	public RhinoSecurityFacility(Type userType)
			:this(SecurityTableStructure.Schema, userType)
    	{
    	}

    	/// <summary>
		/// Initializes a new instance of the <see cref="RhinoSecurityFacility"/> class.
		/// </summary>
		/// <param name="securityTableStructure">The security table structure.</param>
		/// <param name="userType">Type of the user.</param>
    	public RhinoSecurityFacility(SecurityTableStructure securityTableStructure, Type userType)
    	{
    		this.securityTableStructure = securityTableStructure;
    		this.userType = userType;
    	}

    	///<summary>
        ///The custom initialization for the Facility.
        ///</summary>
        protected override void Init()
        {
			Kernel.Register(
				Component.For<INHibernateInitializationAware>()
					.ImplementedBy<NHibernateMappingModifier>()
					.DependsOn(
						Property.ForKey("structure").Eq(securityTableStructure),
						Property.ForKey("userType").Eq(userType)
					),
				Component.For<AddCachingInterceptor>(),
                Component.For<IAuthorizationService>()
                    .ImplementedBy<AuthorizationService>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere,
                Component.For<IAuthorizationRepository>()
                    .ImplementedBy<AuthorizationRepository>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere,
                Component.For<IPermissionsBuilderService>()
                    .ImplementedBy<PermissionsBuilderService>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere,
                Component.For<IPermissionsService>()
                    .ImplementedBy<PermissionsService>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere
				);

            if (Kernel.HasComponent(typeof(IValidatorRegistry)) == false)
            {
            	Kernel.Register(
            		Component.For<IValidatorRegistry>()
            			.ImplementedBy<CachedValidationRegistry>()
            		);
            }

            if (Kernel.HasComponent(typeof(ValidatorRunner)) == false)
            {
            	Kernel.Register(
            		Component.For<ValidatorRunner>()
            		);
            }
        }
    }
}
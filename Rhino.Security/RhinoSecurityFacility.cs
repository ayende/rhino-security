namespace Rhino.Security
{
    using Castle.Components.Validator;
    using Castle.Core;
    using Castle.Core.Interceptor;
    using Castle.MicroKernel.Facilities;
    using Castle.MicroKernel.Registration;
    using Commons;

    /// <summary>
    /// Setup everything necessary for Rhino Security to function
    /// </summary>
    public class RhinoSecurityFacility : AbstractFacility
    {
        ///<summary>
        ///The custom initialization for the Facility.
        ///</summary>
        protected override void Init()
        {
			Kernel.Register(
				Component.ForService<AddCachingInterceptor>(),
                Component.ForService<IAuthorizationService>()
                    .ImplementedBy<AuthorizationService>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere,
                Component.ForService<IAuthorizationRepository>()
                    .ImplementedBy<AuthorizationRepository>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere,
                Component.ForService<IPermissionsBuilderService>()
                    .ImplementedBy<PermissionsBuilderService>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere,
                Component.ForService<IPermissionsService>()
                    .ImplementedBy<PermissionsService>()
                    .Interceptors(new InterceptorReference(typeof(AddCachingInterceptor))).Anywhere
				);

            if (Kernel.HasComponent(typeof(IValidatorRegistry)) == false)
            {
            	Kernel.Register(
            		Component.ForService<IValidatorRegistry>()
            			.ImplementedBy<CachedValidationRegistry>()
            		);
            }

            if (Kernel.HasComponent(typeof(ValidatorRunner)) == false)
            {
            	Kernel.Register(
            		Component.ForService<ValidatorRunner>()
            		);
            }
        }
    }
}
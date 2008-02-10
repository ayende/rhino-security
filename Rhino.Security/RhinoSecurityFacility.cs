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
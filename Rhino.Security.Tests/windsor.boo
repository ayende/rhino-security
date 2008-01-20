import System.Reflection
import Rhino.Security
import Rhino.Security.Tests
import Castle.Components.Validator

component IAuthorizationService, AuthorizationService
component IAuthorizationEditingService, AuthorizationEditingService
component IPermissionsBuilderService , PermissionsBuilderService 
component ValidatorRunner
component IValidatorRegistry, CachedValidationRegistry
component IPermissionsService, PermissionsService

component IEntitySecurityKeyExtractor of Account, AccountSecurityKeyExtractor

component IRepository, ARRepository
component IUnitOfWorkFactory, ActiveRecordUnitOfWorkFactory
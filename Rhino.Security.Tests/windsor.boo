import System.Reflection
import Rhino.Security
import Rhino.Security.Tests
import Castle.Components.Validator

facility RhinoSecurityFacility

component IEntityInformationExtractor of Account, AccountInfromationExtractor

component IRepository, ARRepository
component IUnitOfWorkFactory, ActiveRecordUnitOfWorkFactory
import System.Reflection
import Rhino.Security
import Rhino.Commons from Rhino.Commons.NHibernate as nh
import Rhino.Security.Tests
import Castle.Components.Validator

facility RhinoSecurityFacility

component INHibernateInitializationAware, EnableTestCaching

component IEntityInformationExtractor of Account, AccountInfromationExtractor

component IRepository, ARRepository
component IUnitOfWorkFactory, ActiveRecordUnitOfWorkFactory
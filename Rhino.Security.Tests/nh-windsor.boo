import System.Reflection
import Rhino.Security
import Rhino.Security.Interfaces
import Rhino.Commons from Rhino.Commons.NHibernate as nh
import Rhino.Security.Tests
import Rhino.Security.ActiveRecord
import Castle.Components.Validator

facility RhinoSecurityFacility:
	securityTableStructure = SecurityTableStructure.Prefix
	userType = User

component INHibernateInitializationAware, EnableTestCaching

component IEntityInformationExtractor of Account, AccountInfromationExtractor

component IRepository, NHRepository
component IUnitOfWorkFactory, NHibernateUnitOfWorkFactory
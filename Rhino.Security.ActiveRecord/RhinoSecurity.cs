using System;
using Rhino.Security.Model;

namespace Rhino.Security.ActiveRecord
{
	internal class RhinoSecurity
	{
		internal static Type[] Entities = new Type[]
		{
			typeof (EntitiesGroup),
			typeof (EntityReference),
			typeof (EntityType),
			typeof (Operation),
			typeof (Permission),
			typeof (UsersGroup)
		};
	}
}
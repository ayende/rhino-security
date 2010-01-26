using System;
using System.Collections.Generic;
using System.IO;
using Castle.ActiveRecord;
using Castle.ActiveRecord.Framework.Internal;
using Rhino.Security.ActiveRecord;

[assembly: RegisterRhinoSecurityMapping]

namespace Rhino.Security.ActiveRecord
{
	public class RegisterRhinoSecurityMappingAttribute : RawXmlMappingAttribute
	{
		public override string[] GetMappings()
		{
			List<string> mapping = new List<string>();
			ActiveRecordModelBuilder builder = new ActiveRecordModelBuilder();
			foreach (Type type in RhinoSecurity.Entities)
			{
				builder.CreateDummyModelFor(type);
				Stream stream = type.Assembly.GetManifestResourceStream(type.FullName+".hbm.xml");
				if (stream == null)
					continue;
				using (StreamReader reader = new StreamReader(stream))
					mapping.Add(reader.ReadToEnd());
			}
			return mapping.ToArray();
		}
	}
}
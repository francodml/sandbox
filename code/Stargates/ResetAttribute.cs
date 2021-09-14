using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Sandbox;

namespace winsandbox.Stargates
{
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
	public class ResetAttribute : Attribute
	{
		public object DefaultValue;
		public ResetAttribute( object DefaultValue )
		{
			this.DefaultValue = DefaultValue;
		}

		public static void ResetAll( object instance )
		{
			var props = Reflection.GetProperties( instance ).Where( x => x.GetCustomAttribute<ResetAttribute>() != null );
			foreach ( var prop in props )
			{
				var defaultValue = prop.GetCustomAttribute<ResetAttribute>().DefaultValue;
				prop.SetValue( instance, defaultValue );
			}
		}
	}
}

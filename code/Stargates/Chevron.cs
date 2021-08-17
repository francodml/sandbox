using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	public partial class Chevron : ModelEntity
	{
		public bool Engaged { get; private set; }
		private PointLightEntity light;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/stargates/chevron_sg1.vmdl" );

			light = new();
			light.Position = this.Position + (Rotation.Forward * 15);
			light.SetParent( this );
			light.SetLightColor( Color.Parse( "#FF7F32" ).GetValueOrDefault() );
			light.Brightness = 2;
			light.Enabled = false;
		}

		public void Toggle()
		{
			Toggle( !Engaged );
		}

		public void Toggle(bool state)
		{
			Engaged = state;
			SetMaterialGroup( state ? 1 : 0 );
			light.Enabled = state;
		}
	}
}

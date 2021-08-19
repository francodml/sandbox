using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	public partial class Chevron : AnimEntity
	{
		public bool Engaged { get; private set; }
		public bool Continue { get; private set; }
		public Chevron OtherChevron { get; set; }
		private PointLightEntity light;
		private bool stayLit;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/stargates/chevron_sg1.vmdl" );

			light = new();
			light.Position = this.Position + (Rotation.Forward * 15);
			light.SetParent( this, "chevron_top" );
			light.SetLightColor( Color.Parse( "#FF7F32" ).GetValueOrDefault() );
			light.Brightness = 2;
			light.Enabled = false;
			UseAnimGraph = true;
			Continue = true;
		}

		public void Toggle( bool silent = true )
		{
			Toggle( !Engaged, silent );
		}

		public void Toggle (bool state, bool silent = true )
		{
			if ( !this.IsValid() )
				return;
			Engaged = state;
			SetMaterialGroup( state ? 1 : 0 );
			if ( !silent )
				PlaySound( $"stargates.milkyway.chevron{(state ? "" : ".close")}" );
			light.Enabled = state;
		}

		public async void Animate(bool StayLit = false)
		{
			Continue = false;
			SetAnimBool( "TriggerLock", true );
			stayLit = StayLit;
			await GameTask.Delay( 10 );
			SetAnimBool( "TriggerLock", false );
		}

		public override void OnAnimGraphCreated()
		{
			base.OnAnimGraphCreated();

			SetAnimBool( "TriggerLock", false );
		}

		public override void OnAnimGraphTag( string tag, AnimGraphTagEvent fireMode )
		{
			base.OnAnimGraphTag( tag, fireMode );

			if ( !IsServer )
				return;
			if (tag == "RegularSound" && fireMode != AnimGraphTagEvent.End )
			{
				PlaySound("stargates.milkyway.chevron");
			}

			if ( tag == "OpenSound" && fireMode != AnimGraphTagEvent.End )
			{
				PlaySound( "stargates.milkyway.chevron.open" );
			}

			if ( tag == "CloseSound" && fireMode != AnimGraphTagEvent.End )
			{
				PlaySound( "stargates.milkyway.chevron.close" );
			}

			if (tag == "Lit" )
			{
				if ( stayLit && Engaged )
					return;
				Toggle(false);
				if ( OtherChevron != null)
				{
					OtherChevron.Toggle();
					OtherChevron = null;
				}
			}
			if (tag == "CooldownEnd" && fireMode == AnimGraphTagEvent.End )
			{
				Continue = true;
			}
		}
	}
}

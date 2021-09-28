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
		public bool Engaged { get; private set; } = false;
		public bool Continue { get; private set; } = true;
		public Chevron OtherChevron { get; set; }
		public bool IsFinalLock { get; set; } = false;
		public bool FailedLock { get; set; } = false;
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
			light.Brightness = 3;
			light.Range = 128;
			light.Enabled = false;
			UseAnimGraph = true;
		}

		public void Toggle( bool silent = true )
		{
			Toggle( !Engaged, silent );
		}

		public void Toggle ( bool state, bool silent = true )
		{
			if ( !this.IsValid() )
				return;
			Engaged = state;
			SetMaterialGroup( state ? 1 : 0 );
			if ( !silent )
				PlaySound( $"stargates.milkyway.chevron.{(state ? "open" : "close")}" );
			light.Enabled = state;
		}

		public async void Animate(bool StayLit = false)
		{
			Continue = false;
			SetAnimBool( "IsFinalLock", IsFinalLock );
			SetAnimBool( "FailedLock", FailedLock );
			SetAnimBool( "TriggerLock", true );
			stayLit = StayLit;
			await GameTask.Delay( 1000 );
			SetAnimBool( "TriggerLock", false );
		}

		protected override void OnAnimGraphCreated()
		{
			base.OnAnimGraphCreated();

			ResetAnimParams();

		}

		protected override void OnAnimGraphTag( string tag, AnimGraphTagEvent fireMode )
		{
			base.OnAnimGraphTag( tag, fireMode );

			if ( !IsServer )
				return;
			if (tag == "RegularSound" && fireMode != AnimGraphTagEvent.End )
			{
				PlaySound("stargates.milkyway.chevron.open");
			}

			if ( tag == "OpenSound" && fireMode != AnimGraphTagEvent.End )
			{
				PlaySound( "stargates.milkyway.chevron.open" );
			}

			if ( tag == "CloseSound" && fireMode != AnimGraphTagEvent.End )
			{
				PlaySound( "stargates.milkyway.chevron.close" );
			}

			if ( tag == "Lit" )
			{
				if ( stayLit && Engaged )
					return;
				Toggle((fireMode == AnimGraphTagEvent.End || IsFinalLock));
				if ( OtherChevron != null )
				{
					OtherChevron.Toggle();
					OtherChevron = null;
				}
			}
			if (tag == "CooldownEnd" && fireMode == AnimGraphTagEvent.End )
			{
				Continue = true;
				IsFinalLock = false;
				FailedLock = false;
			}
		}
	}
}

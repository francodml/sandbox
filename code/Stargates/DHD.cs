using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	[Library("ent_dhd", Spawnable = true, Title = "Milky Way DHD")]
	public partial class DHD : ModelEntity
	{
		public const string GlyphOrder = "0IHGFEDCBA987654321J@ZYXWVUTSRQP#ONMLK";
		public Stargate Stargate { get; set; }
		[Net]public string CurrentAddress { get; set; } = "";
		public bool Busy { get; private set; } = false;
		private List<DHDKey> Keys = new();
		private DHDKey SubmitButton;
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/dhds/dial_base.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			for (int j = 0; j < 2; j++ )
			{
				for (int i = 1; i <= 19; i++ )
				{
					var btn = new DHDKey( $"models/dhds/{(j == 0 ? "small" : "large")}_key.vmdl", this )
					{
						Position = Position,
						Rotation = Transform.RotationToLocal( Rotation.FromYaw( 18.9f * (i - 1) ) ),
						Glyph = GlyphOrder[(i-1) + (19 * j)].ToString()
					};
					btn.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
					Keys.Add( btn );
				}
			}

			SubmitButton = new DHDKey( "models/dhds/submit_key.vmdl", this )
			{
				Position = Position,
				Rotation = Rotation,
				Glyph = "SUBMIT"
			};
			SubmitButton.SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

			FindGate();
		}

		private async void FindGate()
		{
			if ( IsServer )
			{
				await Task.NextPhysicsFrame();
				var sgs = All.OfType<Stargate>();
				var previousLeastDistance = 50000f;
				Stargate previousLDGate = null;
				foreach ( Stargate sg in sgs )
				{
					if ( sg.Position.Distance( Position ) <= previousLeastDistance )
					{
						previousLeastDistance = sg.Position.Distance( Position );
						previousLDGate = sg;
					}
				}
				if ( previousLDGate != null )
				{
					Stargate = previousLDGate;
					Stargate.DHD = this;
				}
			}
		}

		public void KeyPress( string glyph, bool active )
		{ 
			//CurrentAddress += glyph;
			if ( Stargate != null )
			{
				Stargate.DHDKeypress( glyph, active );
			}
		}

		public void Reset()
		{
			foreach( DHDKey k in Keys.Where(x => x.Active) )
			{
				k.Toggle( false );
			}
			SubmitButton.Toggle( false );
		}

		[Event.Frame]
		public void OnFrame()
		{
			DebugOverlay.Text( Position , CurrentAddress );
		}
	}
}

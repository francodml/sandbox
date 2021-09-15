using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	[Library( "ent_dhd", Spawnable = true, Title = "Milky Way DHD" )]
	[Hammer.EditorModel( "models/dhds/dhd.vmdl" )]
	public partial class DHD : ModelEntity
	{
		public const string GlyphOrder = "0IHGFEDCBA987654321J@ZYXWVUTSRQP#ONMLK";
		[Property("Stargate", "The Stargate this DHD is linked to", Group = "Stargate", FGDType = "target_destination")]
		public string hammer_stargate { get; set; }
		public Stargate Stargate { get; set; }
		[Net] public string CurrentAddress { get; set; } = "";
		public bool Active { get; private set; } = false;
		public bool Busy { get; private set; } = false;
		public bool DenyInput
		{
			get
			{
				if ( Stargate == null )
					return false;
				return Stargate.DenyDHDInput;
			}
		}
		public string PointOfOrigin =>  Stargate != null ? Stargate.PointOfOrigin : "#";

		private List<DHDKey> Keys = new();
		private DHDKey SubmitButton;
		private Dictionary<string,int> AddressIndexMap = new();

		public override void Spawn()
		{
			base.Spawn();

			if ( !string.IsNullOrEmpty(hammer_stargate) )
			{
				Stargate = FindAllByName( hammer_stargate ).OfType<Stargate>().FirstOrDefault();
			}


			SetModel( "models/dhds/dhd.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
			PhysicsBody.BodyType = PhysicsBodyType.Static;
			Transform dialcentre = GetAttachment( "dial_centre" ).GetValueOrDefault();

			for (int j = 0; j < 2; j++ )
			{
				for (int i = 1; i <= 19; i++ )
				{
					var btn = new DHDKey( $"models/dhds/{(j == 0 ? "small" : "large")}_key.vmdl", this )
					{
						Scale = Scale,
						Position = dialcentre.Position,
						Rotation = dialcentre.Rotation * Rotation.FromYaw( (360.0f / 19.0f) * (i - 1) ),
						Glyph = GlyphOrder[(i-1) + (19 * j)].ToString()
					};
					btn.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
					PhysicsBody.BodyType = PhysicsBodyType.Static;
					btn.SetBodyGroup( "glyph", i - 1 );
					Keys.Add( btn );
				}
			}

			SubmitButton = new DHDKey( "models/dhds/submit_key.vmdl", this )
			{
				Scale = Scale,
				Position = dialcentre.Position,
				Rotation = dialcentre.Rotation,
				Glyph = "SUBMIT"
			};
			SubmitButton.SetupPhysicsFromModel(PhysicsMotionType.Dynamic);

			if (Stargate == null)
				FindGate();
			else
				Stargate.DHD = this;
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
				else
					return;

				if ( Stargate.OtherAddress != null )
					UpdateFromGate();
			}
		}

		public void KeyPress( string glyph, bool active )
		{
			if ( Stargate.DHD != this )
				Stargate.DHD = this;
			if ( Stargate != null )
			{
				Stargate.DHDKeypress( glyph, active );
			}
			if ( active )
			{
				if ( !Active )
					Active = true;
				if ( CurrentAddress.Length == 8 && glyph != PointOfOrigin & glyph != "SUBMIT" )
					return;
				CurrentAddress += glyph;
				AddressIndexMap[glyph] = AddressIndexMap.Count;
			}
			else
			{
				if ( !AddressIndexMap.ContainsKey( glyph ) )
					return;
				var index = AddressIndexMap[glyph];
				CurrentAddress = CurrentAddress.Remove( index, 1 );
				if ( CurrentAddress.Length == 0 )
					Active = false;
				RegenerateIndexMap();
			}
		}

		public void SetKey( string glyph, bool state )
		{
			var key = Keys.Where( x => x.Glyph == glyph ).FirstOrDefault();
			if (key != null && key.IsValid() )
			{
				key.Toggle(state);
			}
		}

		private void RegenerateIndexMap()
		{
			if ( CurrentAddress.Length == 0 )
				return;
			AddressIndexMap = new();
			for ( int i = 0; i < CurrentAddress.Length; i++ )
			{
				AddressIndexMap[CurrentAddress[i].ToString()] = i;
			}
		}

		public void Reset()
		{
			if ( !IsServer )
				return;
			AddressIndexMap = new();
			foreach( DHDKey k in Keys.Where(x => x.Active) )
			{
				k.Toggle( false );
			}
			SubmitButton.Toggle( false );
			CurrentAddress = "";
			Active = false;
		}

		public void UpdateFromGate()
		{
			foreach ( char c in Stargate.OtherAddress )
				SetKey( c.ToString(), true );
			CurrentAddress = Stargate.OtherAddress;
			RegenerateIndexMap();

			if( Stargate.Connection != ConnectionType.None )
			{
				SetKey( PointOfOrigin, true );
				SubmitButton.Toggle( true );
				Active = true;
			}
		}

		[Event.Frame]
		public void OnFrame()
		{
			DebugOverlay.Text( Position , CurrentAddress );
		}

		[Event.Entity.PostSpawn]
		public static void PostMapSpawn()
		{
			foreach ( DHD dhd in All.OfType<DHD>() )
				dhd.PhysicsBody.BodyType = PhysicsBodyType.Keyframed;
		}
	}
}

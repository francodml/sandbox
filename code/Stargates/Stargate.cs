using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	[Library("ent_stargate", Spawnable = true, Title = "Milky Way Stargate")]
	[Hammer.EditorModel("models/stargates/stargate.vmdl")]
	public partial class Stargate : AnimEntity, IUse
	{
		[Net]
		[Property("Address", Group = "Stargate")]
		public string Address { get; private set; }
		[Net]
		public bool Busy { get; private set; }

		private string OtherAddress;
		private Stargate OtherGate;

		private EventHorizon eventHorizon;
		private Ring ring;
		public List<Chevron> chevrons = new();

		public override void Spawn()
		{
			base.Spawn();
			Transmit = TransmitType.Always;

			Tags.Add( "IsStargate" );
			SetModel( "models/stargates/stargate.vmdl" );

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
			PhysicsBody.BodyType = PhysicsBodyType.Static;

			if ( IsServer && string.IsNullOrEmpty( Address ) )
			{
				List<char> PotentialSymbols = new();
				PotentialSymbols.AddRange( Utils.AddressSymbols );
				for ( int i = 0; i < 6; i++ )
				{
					int random = Rand.Int( 0, PotentialSymbols.Count - 1 );
					Address += PotentialSymbols[random];
					PotentialSymbols.RemoveAt( random );
				}
			}

			var centre = GetAttachment( "Centre" ).GetValueOrDefault();
			eventHorizon = new();
			eventHorizon.Position = Position;
			eventHorizon.Rotation = Rotation;
			eventHorizon.SetParent( this );

			ring = new();
			ring.Position = centre.Position;
			ring.Rotation = Rotation;
			ring.SetParent( this );
			
			for (int i = 0; i < 9; i++ )
			{
				var ent = new Chevron();
				var attach = GetAttachment( $"Chevron0{i+1}" ).GetValueOrDefault();
				ent.Position = attach.Position;
				ent.Rotation = attach.Rotation;
				ent.SetParent( this );
				ent.Transmit = TransmitType.Always;
				chevrons.Add( ent );
			}

		}
		public bool IsUsable( Entity user ) => true;
		public bool OnUse( Entity user )
		{
			OpenMenu( To.Single( user ) );
			return false;
		}

		[ClientRpc]
		public void OpenMenu()
		{
			var menu = Local.Hud.AddChild<GateMenu>();
			menu.SetGate( this );
		}

		public Stargate FindGate(string address = null)
		{
			return All.OfType<Stargate>().Where( x => x.Address == address && x.Address != Address).FirstOrDefault();
		}

		public bool Connect(string address = null)
		{
			if ( !IsServer )
				return false;
			if ( address == null && OtherAddress == null )
				return false;
			if ( Busy )
				return false;

			var otherGate = FindGate( address == null ?  OtherAddress : address);
			if ( otherGate == null || !otherGate.IsValid() || otherGate.Busy ) //TODO: Handle busy gate as fail
				return false;
			OtherAddress = address;
			OtherGate = otherGate;

			OtherGate.ConnectIncoming( this );

			eventHorizon.Enable();

			Busy = true;
			SetChevrons( true );

			return true;
		}

		public void ConnectIncoming(Stargate other)
		{
			OtherAddress = other.Address;
			OtherGate = other;
			Busy = true;
			eventHorizon.Enable();
			SetChevrons( true );
		}

		public void Disconnect()
		{
			if ( !IsServer | OtherGate == null )
				return;

			OtherAddress = string.Empty;
			eventHorizon.Disable();
			Busy = false;

			if ( OtherGate.IsValid() && OtherGate.Busy )
				OtherGate.Disconnect();

			OtherGate = null;
			SetChevrons( false );
		}

		public void Reset()
		{
			SetChevrons( false );
			currentChevron = 0;
			dialling = false;
		}

		public void SetChevrons(bool state)
		{
			if ( IsClient )
				return;
			foreach( Chevron chev in chevrons )
			{
				chev.Toggle(state: state);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Disconnect();
		}

		public void RotateRing( float degrees )
		{
			ring.SetSpecificAngle(degrees);
		}

		public async void Teleport(Entity ent)
		{
			if ( !IsServer )
				return;

			if ( ent is SandboxPlayer ply )
			{
				var controller = new UselessPlayerController();
				var oldController = ply.Controller;
				ply.Controller = controller;

				var DeltaAngle = OtherGate.eventHorizon.Rotation.Angles() - this.eventHorizon.Rotation.Angles();


				ply.EyeRot = Rotation.From( ply.EyeRot.Angles() + new Angles( 0, DeltaAngle.yaw+180, 0 ) );
				ply.Rotation = Rotation.From( ply.EyeRot.Angles() + new Angles( 0, DeltaAngle.yaw+180, 0 ) );

				Log.Info( ply.GetActiveController() );
				Log.Info("player came through, tried to rotate");
				await GameTask.NextPhysicsFrame();
				ply.Controller = oldController;
			}

			ent.Position = OtherGate.Position + OtherGate.Rotation.Forward * 80;
			ent.ResetInterpolation();

			//ent.Velocity = new Vector3( 0, 0, 0 );
		}

		[ClientRpc]
		public void fuck( )
		{
			Log.Info( Input.Rotation );
			Log.Info( Input.Rotation );
		}

		[ServerCmd]
		public static void UI_Disconnect( int GateIdent )
		{
			if ( Entity.FindByIndex(GateIdent) is Stargate g && g.IsValid() )
			{
				if ( !g.Busy )
				{
					g.Reset();
					return;
				}
				g.Disconnect();
			}
		}

		[ServerCmd]
		public static void UI_Connect( int GateIdent, string address )
		{
			if ( Entity.FindByIndex( GateIdent ) is Stargate g && g.IsValid() )
			{
				/*g.OtherAddress = address;
				g.dialling = true;*/
				g.Connect( address );
			}
		}
	}
}

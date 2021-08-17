using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	[Library("ent_stargate", Spawnable = true, Title = "Stargate")]
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
		private List<Chevron> chevrons = new();
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
				for ( int i = 0; i < 7; i++ )
				{
					int random = Rand.Int( 0, PotentialSymbols.Count - 1 );
					Address += PotentialSymbols[random];
					PotentialSymbols.RemoveAt( random );
				}
			}
			eventHorizon = new();
			eventHorizon.Position = Position;
			eventHorizon.Rotation = Rotation;
			eventHorizon.SetParent( this );
			
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

		public async void Glow()
		{
			GlowActive = true;
			await Task.DelaySeconds( 5 );
			GlowActive = false;
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

			var otherGate = FindGate( address );
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

		public void SetChevrons(bool state)
		{
			if ( IsClient )
				return;
			foreach( Chevron chev in chevrons )
			{
				chev.Toggle();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			Disconnect();
		}	

		public void Teleport(Entity ent)
		{
			if ( !IsServer )
				return;

			if ( true )
			{
				ent.Rotation = Rotation.LookAt( (OtherGate.Rotation.Forward * 100 - ent.Position).WithZ( 0 ).Normal );
				ent.EyeRot = ent.Rotation;
			}

			ent.Position = OtherGate.Position + OtherGate.Rotation.Forward * 100;

			ent.Velocity = new Vector3( 0, 0, 0 );
		}

		[Net]
		public bool ShouldDisconnect { get; set; }

		[Event.Tick]
		public void Tick()
		{
			//Log.Info( $"ShouldDisconnect:{ShouldDisconnect}" );
			if ( ShouldDisconnect == true)
			{
				ShouldDisconnect = false;
				Log.Info($"{IsClient} {IsServer} Attempted Disconnect");
				Disconnect();
			}
		}
	}
}

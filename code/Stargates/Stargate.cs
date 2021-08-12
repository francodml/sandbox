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
		public override void Spawn()
		{
			base.Spawn();

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
			Log.Info($"IsStargate: {Tags.Has("IsStargate")}");

		}
		public bool IsUsable( Entity user ) => true;
		public bool OnUse( Entity user )
		{
			ChatBox.AddInformation(To.Single(user),Address);
			Log.Info( Address );
			CopyAddressToClipboard( To.Single( user ) );
			return false;
		}

		[ClientRpc]
		public void CopyAddressToClipboard()
		{
			Local.Hud.AddChild<GateMenu>();
			Clipboard.SetText( Address );
		}

		public async void Glow()
		{
			GlowActive = true;
			await Task.DelaySeconds( 5 );
			GlowActive = false;
		}

		public Stargate FindGate(string address = null)
		{
			if ( address == null && OtherAddress != null )
				address = OtherAddress;

			return (Stargate)Entity.All.Where( x => x is Stargate a && a.Address == address ).FirstOrDefault();
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
			if ( otherGate == null || !otherGate.IsValid() || otherGate.Busy )
				return false;
			OtherAddress = address;
			OtherGate = otherGate;

			OtherGate.ConnectIncoming( this );

			eventHorizon.Enable();

			Busy = true;

			return true;
		}

		public void ConnectIncoming(Stargate other)
		{
			OtherAddress = other.Address;
			OtherGate = other;
			Busy = true;
			eventHorizon.Enable();
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

			if ( ent is SandboxPlayer ply )
			{

			}

			/*ent.EyeRot = ent.EyeRot.RotateAroundAxis( new Vector3( 0, 0, 1 ), 180);
			ent.EyeRotLocal = ent.EyeRotLocal.RotateAroundAxis( new Vector3( 0, 0, 1 ), 180 );*/
			ent.Position = OtherGate.Position + OtherGate.Rotation.Forward * 100;

			ent.Velocity = new Vector3( 0, 0, 0 );
		}
	}
}

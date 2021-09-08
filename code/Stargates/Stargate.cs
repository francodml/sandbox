using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using winsandbox.Stargates.UI;

namespace winsandbox.Stargates
{
	[Library("ent_stargate", Spawnable = true, Title = "Milky Way Stargate")]
	[Hammer.EditorModel("models/stargates/stargate.vmdl")]
	public partial class Stargate : AnimEntity, IUse
	{
		
		[Property("Address", Group = "Stargate")]
		[Net] public string Address { get; private set; }

		[Property( "Point of Origin", Group = "Stargate" )]
		[Net] public string PointOfOrigin { get; private set; } = "#";
		[Net] public bool Busy { get; private set; }
		[Net] public Connection ConnectionType { get; private set; } = Connection.None;
		[Net] public string CurrentRingSymbol => ring.CurrentSymbol;
		public DHD DHD { get; set; }

		[Net] public string OtherAddress { get; set; } = "";
		private Stargate OtherGate;

		private EventHorizon eventHorizon;
		[Net] private Ring ring { get; set; }
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

		internal void RotateRingToSymbol( string symbol )
		{
			ring.RotateToSymbol( symbol );
		}

		public bool OnUse( Entity user )
		{
			OpenMenu( To.Single( user ) );
			return false;
		}

		[ClientRpc]
		public void OpenMenu()
		{
			var menu = Local.Hud.AddChild<GateControlPanel>();
			menu.SetGate( this );
		}

		public Stargate FindGate(string address = null)
		{
			if ( address == null )
				address = OtherAddress;
			return All.OfType<Stargate>().Where( x => x.Address == address && x.Address != Address).FirstOrDefault();
		}

		public bool Connect(string address = null)
		{
			if ( !IsServer )
				return false;
			if ( address == null && OtherAddress == null )
				return false;
			else if ( address == null && OtherAddress != null )
				address = OtherAddress;
			if ( Busy )
				return false;

			var otherGate = FindGate( address == null ?  OtherAddress : address);
			if ( otherGate == null || !otherGate.IsValid() || otherGate.Busy )
			{
				FailConnection();
				return false;
			}
			OtherAddress = address;
			OtherGate = otherGate;

			OtherGate.ConnectIncoming( this );

			eventHorizon.Enable();

			ConnectionType = Connection.Outgoing;
			Busy = true;
			SetChevrons( true );

			if ( DHD != null )
				DHD.UpdateFromGate();

			return true;
		}

		public async void FailConnection()
		{
			PlaySound( "stargates.milkyway.fail" );
			await Task.DelaySeconds( 3.5f );
			PlaySound( "stargates.milkyway.chevron.close" );
			Reset();
		}

		public void ConnectIncoming(Stargate other)
		{
			ConnectionType = Connection.Incoming;
			OtherAddress = other.Address;
			OtherGate = other;
			Busy = true;
			eventHorizon.Enable();
			SetChevrons( true );

			if ( DHD != null )
				DHD.UpdateFromGate();
		}

		public void Disconnect()
		{
			if ( !IsServer | OtherGate == null )
				return;

			ConnectionType = Connection.None;
			OtherAddress = string.Empty;
			eventHorizon.Disable();
			Busy = false;

			if ( OtherGate.IsValid() && OtherGate.Busy )
				OtherGate.Disconnect();

			OtherGate = null;
			SetChevrons( false );
			Reset();
		}

		public void Reset()
		{
			SetChevrons( false );
			currentChevron = 0;
			dialling = false;
			OtherAddress = "";
			chevrons[6].ResetBones();
			ring.Stop();
			AddressIndexMap = null;
			if ( DHD != null )
				DHD.Reset();
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

		public void ToggleRing()
		{
			ring.Toggle();
		}

		public async void Teleport(Entity ent)
		{
			if ( !IsServer )
				return;

			if ( ent is SandboxPlayer ply )
			{
				var controller = new UselessPlayerController();
				var oldController = ply.Controller;
				using ( Prediction.Off() )
				{
					ply.Controller = controller;
				}

				var DeltaAngle = OtherGate.eventHorizon.Rotation.Angles() - this.eventHorizon.Rotation.Angles();


				ply.EyeRot = Rotation.From( ply.EyeRot.Angles() + new Angles( 0, DeltaAngle.yaw+180, 0 ) );
				ply.Rotation = Rotation.From( ply.EyeRot.Angles() + new Angles( 0, DeltaAngle.yaw+180, 0 ) );

				await GameTask.NextPhysicsFrame();
				using ( Prediction.Off() )
				{
					ply.Controller = oldController;
				}
			}
			ent.Position = OtherGate.GetAttachment("Centre").GetValueOrDefault().Position + OtherGate.Rotation.Forward * 100;
			ent.ResetInterpolation();
			OtherGate.PlaySound( "stargates.milkyway.pass" );

			ent.Velocity = OtherGate.Rotation.Forward * ent.Velocity.Length;
		}

		public enum Connection
		{
			None,
			Incoming,
			Outgoing
		}
	}
}

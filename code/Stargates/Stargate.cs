using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		[Net, Property( "Address" ), Category( "Stargate" )] public string Address { get; private set; }

		[Net, Property( "Name" ), Category( "Stargate" )] public string Name { get; private set; }

		[Property( "Point of Origin" ), Category( "Stargate" )]
		[Net] public string PointOfOrigin { get; private set; } = "#";
		[Net, Category( "Stargate" ), Reset( false )] public bool Busy { get; private set; }
		[Net, Category( "Stargate" ), Reset( ConnectionType.None )] public ConnectionType Connection { get; private set; } = ConnectionType.None;
		[Net, Category( "Stargate" )] public string CurrentRingSymbol => ring.CurrentSymbol;
		public DHD DHD { get; set; }
		[Net, Category( "Stargate" ), Reset( GateState.Idle )] public GateState State { get; private set; } = GateState.Idle;

		[Net, Reset( "" )] public string OtherAddress { get; set; } = "";
		[Net, Reset( null )] private Stargate OtherGate { get; set; }

		private EventHorizon eventHorizon;
		[Net] private Ring ring { get; set; }
		private List<Chevron> chevrons = new();
		private Chevron TopChevron => chevrons[8];

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
			eventHorizon.Position = centre.Position;
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
			if ( user is Player p )
			{
				p.Controller = new StargateMenuPC(p.EyeRotation);
			}
			return false;
		}

		[ClientRpc]
		public void OpenMenu()
		{
			if ( Local.Hud.ChildrenOfType<GateControlPanel>().Count() != 0 )
				return;
			var menu = new GateControlPanel( this );
			Local.Pawn.Tags.Add( "Stargate.PanelOpen" );
			Local.Hud.AddChild( menu );
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

			State = GateState.Open;
			Connection = ConnectionType.Outgoing;
			Busy = true;
			SetChevrons( true );

			if ( DHD != null )
				DHD.UpdateFromGate();

			return true;
		}

		public async void FailConnection()
		{
			Busy = true;
			State = GateState.Closing;
			PlaySound( "stargates.milkyway.fail" );
			await Task.DelaySeconds( 3.5f );
			Reset();
		}

		public void ConnectIncoming(Stargate other)
		{
			State = GateState.Open;
			Connection = ConnectionType.Incoming;
			OtherAddress = other.Address;
			OtherGate = other;
			Busy = true;
			eventHorizon.Enable();
			SetChevrons( true );

			if ( DHD != null )
				DHD.UpdateFromGate();
		}

		public async void Disconnect()
		{
			if ( !IsServer | OtherGate == null )
				return;

			State = GateState.Closing;
			Busy = false;

			if ( OtherGate.IsValid() && OtherGate.Busy )
				OtherGate.Disconnect();

			OtherGate = null;
			await eventHorizon.Disable();

			Reset();
		}

		public async void Reset()
		{
			if ( State == GateState.Dialling )
			{
				PlaySound( "stargates.milkyway.runningfail" );
				await Task.DelaySeconds( 1 );
			}
			ResetAttribute.ResetAll( this );
			SetChevrons( false, true );
			TopChevron.ResetBones();
			ring.Stop();
			AddressIndexMap = null;
			if ( DHD != null )
				DHD.Reset();
		}

		public void SetChevrons(bool state, bool sound = false)
		{
			if ( IsClient )
				return;
			if ( !IsValid )
				return;
			if ( sound )
				PlaySound( $"stargates.milkyway.chevron.{(state ? "open" : "close")}" );
			foreach ( Chevron chev in chevrons )
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


				ply.EyeRotation = Rotation.From( ply.EyeRotation.Angles() + new Angles( 0, DeltaAngle.yaw+180, 0 ) );
				ply.Rotation = Rotation.From( ply.EyeRotation.Angles() + new Angles( 0, DeltaAngle.yaw+180, 0 ) );

				await GameTask.NextPhysicsFrame();
				using ( Prediction.Off() )
				{
					ply.Controller = oldController;
				}
			}

			var localVelNorm = Transform.NormalToLocal( ent.Velocity.Normal );
			var otherVelNorm = OtherGate.Transform.NormalToWorld( localVelNorm.WithX( -localVelNorm.x ) );

			var localPos = Transform.PointToLocal( ent.Position );
			var otherPos = OtherGate.Transform.PointToWorld( localPos.WithY( -localPos.y ).WithX(70) );

			var localRot = Transform.RotationToLocal( ent.Rotation );
			var otherRot = OtherGate.Transform.RotationToWorld( localRot.RotateAroundAxis( localRot.Up, 180f ) );

			ent.Position = otherPos;
			ent.ResetInterpolation();
			ent.Velocity = otherVelNorm * ent.Velocity.Length;
			OtherGate.PlaySound( "stargates.milkyway.pass" );

		}

		[Event.Entity.PostSpawn]
		public static void PostMapSpawn()
		{
			foreach ( Stargate dhd in All.OfType<Stargate>() )
				dhd.PhysicsBody.BodyType = PhysicsBodyType.Keyframed;
		}
	}
}

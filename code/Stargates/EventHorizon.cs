using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	[Hammer.Skip]
	[Library("ent_stargate_eh", Spawnable = false)]
	partial class EventHorizon : ModelEntity
	{
		[Net]
		public bool Enabled { get; private set; }
		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/stargates/eventhorizon.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
			PhysicsBody.BodyType = PhysicsBodyType.Static;
			EnableSolidCollisions = false;
			UsePhysicsCollision = false;
			EnableAllCollisions = false;
			Enabled = false;
			EnableDrawing = false;
			EnableTouch = true;
		}

		public override void StartTouch( Entity other )
		{
			if ( other.IsWorld || other.GetType() == typeof(PickupTrigger))
				return;
			if ( !Enabled )
				return;
			if (IsServer)
			{
				PlaySound( "stargates.milkyway.pass" );
				Log.Info( $"Touch started with {other}:{other.NetworkIdent}" );
			}
			if ( IsClient )
			{
				var gate = Parent as Stargate;
				ChatBox.AddInformation( $"Touching {gate.Address}" );
			}
		}

		public void Enable()
		{
			Enabled = true;
			EnableDrawing = true;
			PlaySound( "stargates.milkyway.open" );
		}
		public void Disable()
		{
			Enabled = false;
			EnableDrawing = false;
			PlaySound( "stargates.milkyway.close" );
		}
	}
}

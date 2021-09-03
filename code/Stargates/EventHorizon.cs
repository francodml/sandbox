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
			try
			{
				if ( other.IsWorld || Utils.IgnoredTypes[other.GetType()] )
					return;
			}
			catch ( Exception ) { }

			if ( !Enabled )
				return;
			var gate = Parent as Stargate;
			if ( IsServer )
			{
				PlaySound( "stargates.milkyway.pass" );
				if (gate.ConnectionType == Stargate.Connection.Incoming )
				{
					if (other is SandboxPlayer ply )
					{
						ply.OnKilled();
						return;
					}
					other.Delete();
					return;
				}
				gate.Teleport( other );
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	[Hammer.Skip]
	[Library("ent_stargate_eh", Spawnable = false)]
	partial class EventHorizon : ModelEntity
	{
		[Net]
		public bool Enabled { get; private set; }
		[Net] public TimeSince openTime { get; private set; } = 0;
		[Net]public bool Peaked { get; private set; }

		Timeline UVortexTimeline;

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

		public EventHorizon()
		{
			UVortexTimeline = new Timeline()
				.WithKeyframe( 2.0f, 300 )
				.WithKeyframe( 5.0f, 0, EasingFunction.Sine )
				.WithKeyframe( 10.0f, 300, EasingFunction.Sine )
				.WithKeyframe( 7.0f, 0 );
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

				if ( gate.Connection == ConnectionType.Incoming )
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
			Vortex();
		}
		public void Disable()
		{
			Enabled = false;
			EnableDrawing = false;
			PlaySound( "stargates.milkyway.close" );
		}

		private async void Vortex()
		{
			Peaked = false;
			await Task.DelaySeconds( 1.0f );
			openTime = 0;
			SetBodyGroup( "UVortexB", 1 );
			await Task.DelaySeconds( 1.4f );
			Peaked = true;
			await Task.DelaySeconds( 1.4f );
			SetBodyGroup( "UVortexB", 0 );
		}

		[Event.Frame]
		public void OnFrame()
		{
			SceneObject.SetValue( "Time", openTime );

			SceneObject.SetValue( "WorldPos", Position );

			SceneObject.SetValue( "Peaked", Peaked );
		}
	}
}

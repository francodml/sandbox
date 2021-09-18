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

		Timeline CapsuleEndPoint;
		Timeline CapsuleWidth;

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
			CapsuleEndPoint = new Timeline()
				.WithKeyframe( 0.03f, -160 )
				.WithKeyframe( 1.4f, 300, EasingFunction.ExpoOut )
				.WithKeyframe( 2.8f, -200, EasingFunction.Quint );
			CapsuleWidth = new Timeline()
				.WithKeyframe( 0.1f, 160.8f )
				.WithKeyframe( 2.0f, 85, EasingFunction.ExpoOut );
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

			SceneObject.SetValue( "CapsuleEndPoint", new Vector3(CapsuleEndPoint.GetValue(openTime),0,0)*Rotation );
			SceneObject.SetValue( "CapsuleWidth",  CapsuleWidth.GetValue(openTime));
		}
	}
}

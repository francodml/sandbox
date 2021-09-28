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

		Timeline CapsuleEndPoint;
		Timeline CapsuleWidth;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/stargates/eventhorizon.vmdl" );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
			PhysicsBody.BodyType = PhysicsBodyType.Static;
			Transmit = TransmitType.Always;
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
				.WithKeyframe( 0.8f, 350, EasingFunction.ExpoOut )
				.WithKeyframe( 3.0f, -200, EasingFunction.Quint );
			CapsuleWidth = new Timeline()
				.WithKeyframe( 0.03f, 120 )
				.WithKeyframe( 0.3f, 100 )
				.WithKeyframe( 2.0f, 75, EasingFunction.ExpoOut );
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
			ToggleStateClient( To.Everyone, false );
			Vortex();
		}
		public async Task Disable()
		{
			PlaySound( "stargates.milkyway.close" );
			ToggleStateClient( To.Everyone, false );
			await Task.DelaySeconds( 2.6f );
			if ( !this.IsValid | this == null )
				return;
			Enabled = false;
			EnableDrawing = false;
		}

		///<summary>Passes state to the shader. This is used to determine when the event horizon should play the open animation.</summary>
		[ClientRpc]
		public void ToggleStateClient(bool state)
		{
			if ( SceneObject == null || !SceneObject.IsValid() )
				return;
			SceneObject.SetValue( "State", state );
		}

		private async void Vortex()
		{
			await Task.DelaySeconds( 1.0f );
			openTime = 0;
			SetBodyGroup( "UVortexB", 1 );
			await Task.DelaySeconds( 2.8f );
			SetBodyGroup( "UVortexB", 0 );
		}

		[Event.Frame]
		public void OnFrame()
		{
			if ( SceneObject == null || !SceneObject.IsValid() )
				return;
			SceneObject.SetValue( "Time", openTime );

			SceneObject.SetValue( "WorldPos", Position );

			var ass = Matrix.CreateRotation( Rotation );

			SceneObject.SetValue( "CapsuleEndPoint", ass.Transform(new Vector3(CapsuleEndPoint.GetValue(openTime),0,0)) );
			SceneObject.SetValue( "CapsuleWidth",  CapsuleWidth.GetValue(openTime));
		}
	}
}

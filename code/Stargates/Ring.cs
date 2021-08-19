using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	class Ring : ModelEntity
	{
		public bool Rotating { get; private set; }

		private bool ShouldRotateRing = false;
		public float Speed { get; set; }
		public bool Direction { get; set; }

		private Sound spinSound;

		public Ring()
		{
			Speed = 1;
			Direction = false;
			Rotating = false;
		}

		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/stargates/ring.vmdl" );
		}

		public void Start()
		{
			ShouldRotateRing = true;
			Rotating = true;
			spinSound = PlaySound( "stargates.milkyway.ringloop" );
		}

		public void Stop()
		{
			ShouldRotateRing = false;
			Direction = !Direction;
			Rotating = false;
			PlaySound( "stargates.milkyway.chevron.open" );
			spinSound.Stop();
		}

		public void Toggle()
		{
			if (Rotating)
			{
				Stop();
			} else
			{
				Start();
			}
		}

		public void SetSpecificAngle( float degrees )
		{
			LocalRotation = Rotation.FromRoll( degrees );
		}

		[Event.Tick.Server]
		public void Tick()
		{
			if ( ShouldRotateRing )
			{
				LocalRotation = LocalRotation.RotateAroundAxis( LocalRotation.Forward, Speed * (Direction ? 1 : -1) );
			}
		}

	}
}

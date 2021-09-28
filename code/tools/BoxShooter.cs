using Sandbox;
using Sandbox.Tools;

namespace winsandbox.tools
{
	[Library( "tool_barrelgun", Title = "Barrel Shooter", Description = "Shoot barrels", Group = "fun" )]
	public class BoxShooter : BaseTool
	{
		TimeSince timeSinceShoot;

		public override void Simulate()
		{
			if ( Host.IsServer )
			{
				if ( Input.Pressed( InputButton.Attack1 ) )
				{
					ShootBox();
				}

				if ( Input.Down( InputButton.Attack2 ) && timeSinceShoot > 0.02f )
				{
					timeSinceShoot = 0;
					ShootBox();
				}
			}
		}

		void ShootBox()
		{
			var ent = new Prop
			{
				Position = Owner.EyePos + Owner.EyeRot.Forward * 50,
				Rotation = Owner.EyeRot
			};

			ent.SetModel( "models/citizen_props/crate01.vmdl" );
			ent.Velocity = Owner.EyeRot.Forward * 10000;
		}
	}
}

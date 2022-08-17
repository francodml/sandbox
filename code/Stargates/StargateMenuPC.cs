using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	internal partial class StargateMenuPC : BasePlayerController
	{
		public Rotation LockRot { get; set; }
		[Net] public bool MouseClicked { get; private set; }

		public StargateMenuPC()
		{

		}

		public StargateMenuPC( Rotation rot )
		{
			LockRot = rot;
		}

		public override void BuildInput( InputBuilder input )
		{
			base.BuildInput( input );

			//input.ViewAngles = LockRot.Angles();
			MouseClicked = input.Down( InputButton.Attack1 );
			//input.SuppressButton( InputButton.Attack1 );

			input.StopProcessing = true;
		}

		public override void Simulate()
		{
			base.Simulate();

			if ( !Input.Down( InputButton.Attack1 ) )
				return;

			var tr2 = Trace.Ray( Input.Cursor, 1000 )
				.Ignore( Local.Pawn )
				.UseHitboxes()
				.Run();
			if ( tr2.Hit && tr2.Entity is DHDKey d )
			{
				ChatBox.AddInformation( d.Glyph );
			}

			//Rotation = Input.Rotation;
		}

		public override void FrameSimulate()
		{
			base.FrameSimulate();

			//EyeRotation
		}

	}
}

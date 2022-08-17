using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	public partial class UselessPlayerController : WalkController
	{
		public override void BuildInput( InputBuilder input )
		{
			//base.BuildInput( input );

			input.ViewAngles = EyeRotation.Angles();
			input.StopProcessing = true;
		}

		public override void FrameSimulate()
		{
			//base.FrameSimulate();

		}

		public override void Simulate()
		{
			//base.Simulate();
		}

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Tools;

namespace winsandbox.Stargates
{
	[Library("tool_dhdlinker", Title = "DHD Linker", Description = "Link your DHD to your gate", Group = "stargate")]
	public partial class DHDLinkTool : BaseTool
	{
		public Stargate gate;
		public DHD dhd;
		public override void Simulate()
		{
			if ( Host.IsServer )
			{
				if ( Input.Down( InputButton.Attack1 ) )
				{
					var startPos = Owner.EyePosition;
					var dir = Owner.EyeRotation.Forward;

					if ( !Input.Pressed( InputButton.Attack1 ) ) return;

					var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					   .Ignore( Owner )
					   .UseHitboxes()
					   .HitLayer( CollisionLayer.Debris )
					   .Run();
					if ( !tr.Hit || !tr.Entity.IsValid() )
						return;

					switch ( tr.Entity )
					{
						case Stargate sg:
							gate = sg;
							Log.Info( "set stargate" );
							break;
						case DHD dial:
							dhd = dial;
							Log.Info( "set dhd" );
							break;
					}

					if (dhd != null && gate != null )
					{
						dhd.Stargate = gate;
						gate.DHD = dhd;
						Log.Info( "shit set" );
						Sound.FromScreen( "dhd.milkyway.submit" );
						Clear();
					}

					CreateHitEffects( tr.EndPosition );
				}
			}
		}
		private void Clear()
		{
			dhd = null;
			gate = null;
		}
	}
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	public partial class Stargate
	{
		private int currentChevron = 0;
		private bool dialling = false;
		private bool locking = false;

		public void EncodeChevron()
		{
			if ( !IsServer )
				return;
			if ( (currentChevron == 9 || currentChevron == 6) && !chevrons[6].Engaged)
			{
				LockChevron();
				return;
			}
			chevrons[6].OtherChevron = chevrons[currentChevron];
			chevrons[6].Animate();
			currentChevron++;
			if ( OtherAddress.Length > 7 && currentChevron == 6 )
				currentChevron++;
		}

		public void LockChevron()
		{
			chevrons[6].Animate(true);
			currentChevron = 0;
			dialling = false;
			locking = true;
		}

		[ServerCmd]
		public static void UI_Animate( int GateIdent )
		{
			if ( FindByIndex(GateIdent) is Stargate g )
			{
				g.dialling = true;
			}
		}

		[Event.Tick.Server]
		public void Tick()
		{
			if ( dialling && chevrons[6].Continue )
			{
				EncodeChevron();
			}

			if ( locking && chevrons[6].Continue )
			{
				locking = false;
				Connect();
			}
		}

	}
}

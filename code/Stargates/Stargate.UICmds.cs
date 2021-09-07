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

		[ServerCmd]
		public static void UI_Animate( int GateIdent, string address )
		{
			if ( FindByIndex( GateIdent ) is Stargate g )
			{
				g.OtherAddress = address;
				g.dialling = true;
			}
		}

		[ServerCmd]
		public static void UI_Disconnect( int GateIdent )
		{
			if ( Entity.FindByIndex( GateIdent ) is Stargate g && g.IsValid() )
			{
				if ( !g.Busy )
				{
					g.Reset();
					return;
				}
				g.Disconnect();
			}
		}

		[ServerCmd]
		public static void UI_Connect( int GateIdent, string address )
		{
			if ( Entity.FindByIndex( GateIdent ) is Stargate g && g.IsValid() )
			{
				g.Connect( address );
			}
		}

		[ServerCmd]
		public static void UI_ToggleRing( int GateIdent )
		{
			if ( Entity.FindByIndex( GateIdent ) is Stargate g && g.IsValid() )
			{
				g.ToggleRing();
			}
		}

		[ServerCmd]
		public static void UI_ForceReset( int GateIdent )
		{
			if ( Entity.FindByIndex( GateIdent ) is Stargate g && g.IsValid() )
			{
				g.Reset();
			}
		}

		[ServerCmd]
		public static void UI_SetGateAddress ( int GateIdent, string NewAddress )
		{
			if ( Entity.FindByIndex(GateIdent) is Stargate g && g.IsValid() )
			{
				g.Address = NewAddress;
			}
		}

	}
}

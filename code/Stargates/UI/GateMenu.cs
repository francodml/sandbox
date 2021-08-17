using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace winsandbox.Stargates
{
	public class GateMenu : Panel
	{
		public Stargate Gate { get; private set; }
		public Button gateaddress;

		[Library]
		public GateMenu()
		{
			StyleSheet.Load( "Stargates/UI/GateMenu.scss" );

			var titlebar = AddChild<Titlebar>();
			{
				gateaddress = titlebar.Add.Button( "a", "title", () =>
				{
					Clipboard.SetText( Gate.Address );
					ChatBox.AddInformation( "Copied address to clipboard" );
				} );
				titlebar.Add.Button( "Close", "close", () => this.Delete() );
			}

			Add.Button( "Disconnect", () => Gate.ShouldDisconnect = true ); ;
		}

		public void SetGate( Stargate gate )
		{
			this.Gate = gate;
			gateaddress.Text = Gate.Address;
		}

	}
}

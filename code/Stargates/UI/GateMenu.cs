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
		public TextEntry remoteAddress;

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

			var content = Add.Panel( "content" );
			{
				remoteAddress = content.Add.TextEntry( "" );
				remoteAddress.Placeholder = "Remote Address";
				remoteAddress.AddEventListener( "onchange", ( x ) => {

					if (remoteAddress.Text.Length > 7 )
					{
						remoteAddress.Text = remoteAddress.Text.Substring( 0, 7 );
					}

				});

				content.Add.Button( "Connect", () => Stargate.UI_Connect( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );

				content.Add.Button( "Disconnect", () => Stargate.UI_Disconnect( Gate.NetworkIdent ) );

				content.Add.Button( "Animate", () => Stargate.UI_Animate( Gate.NetworkIdent ) );
			}
		}

		public void SetGate( Stargate gate )
		{
			this.Gate = gate;
			gateaddress.Text = Gate.Address;
		}

	}
}

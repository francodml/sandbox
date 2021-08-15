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

			var controlBar = Add.Panel( "controlbar" );
			{
				gateaddress = controlBar.Add.Button( "a", "title", () =>
				{
					Clipboard.SetText( Gate.Address );
					ChatBox.AddInformation( "Copied address to clipboard" );
				} );
				controlBar.AddChild( new PanelGrabHandle( this, new Vector2(200,200) ) );
				controlBar.Add.Button( "Close", "close", () => this.Delete() );
			}

		}

		public void SetGate( Stargate gate )
		{
			this.Gate = gate;
			gateaddress.Text = Gate.Address;
		}

	}
}

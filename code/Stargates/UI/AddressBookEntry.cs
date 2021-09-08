using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace winsandbox.Stargates.UI
{
	[Library]
	public class AddressBookEntry : Button
	{
		public string Address { get; set; } = "";
		public string Name { get; set; } = "";
		public GateControlPanel ControlPanel { get; set; }
		public AddressBookEntry(Stargate gate, GateControlPanel controlPanel)
		{

			StyleSheet.Load( "Stargates/UI/AddressBookEntry.scss" );
			Address = gate.Address;
			Name = string.IsNullOrEmpty(gate.Name) ? "Unnamed Stargate" : gate.Name;
			ControlPanel = controlPanel;

			var container = AddChild<Panel>( "rowcontainer" );
			{
				container.Add.Label( Address, "address" );
				container.Add.Label( Name, "fullwidth" );
				container.Add.Label( "Local" );
			}

		}

		protected override void OnDoubleClick( MousePanelEvent e )
		{
			base.OnDoubleClick( e );
			Stargate.UI_Animate( ControlPanel.Gate.NetworkIdent, Address );
		}

		protected override void OnClick( MousePanelEvent e )
		{
			base.OnClick( e );
			ControlPanel.remoteAddress.Text =  Address;
		}

	}
}

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
	public class GateControlPanel : Panel
	{
		public Stargate Gate { get; private set; }
		private TextEntry ourAddress;
		private TextEntry remoteAddress;

		public GateControlPanel()
		{
			StyleSheet.Load( "Stargates/UI/GateControlPanel.scss" );

			var titlebar = AddChild<Titlebar>();
			{
				titlebar.Add.Label( "Stargate Control Panel", "title" );
				titlebar.Add.Button( "Close", "close", () => this.Delete() );
			}

			var content = AddChild<Panel>("content");

			var left = content.AddChild<Panel>( "column left" );
			{
				var addressContainer = left.AddChild<Panel>( "rowcontainer" );
				{
					ourAddress = addressContainer.Add.TextEntry( "" );
					ourAddress.AddClass( "addressinput" );
					ourAddress.Placeholder = "Gate Address";
					ourAddress.AddEventListener( "onchange", () => ourAddress.Text = CleanupAddress( ourAddress.Text ) );

					addressContainer.Add.Button( "Set", () =>
					{
						if ( ourAddress.Text.Length == 6 )
						{
							Sound.FromScreen( "stargate.ui.confirm" );
							Stargate.UI_SetGateAddress( Gate.NetworkIdent, ourAddress.Text.ToUpper() );
						}
					} );
				}
			}

			var middle = content.AddChild<Panel>( "column middle" );
			{
				middle.Add.Label( "awxo" );
			}

			var right = content.AddChild<Panel>( "column right" );
			{
				var addressContainer = right.AddChild<Panel>( "rowcontainer" );
				{
					remoteAddress = addressContainer.Add.TextEntry( "" );
					remoteAddress.AddClass( "addressinput" );
					remoteAddress.Placeholder = "Remote Address";
					remoteAddress.AddEventListener( "onchange", () => remoteAddress.Text = CleanupAddress( remoteAddress.Text ) );

					addressContainer.Add.Button( "Slow", "dial slowdial",() => Stargate.UI_Animate( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );
					addressContainer.Add.Button( "Fast", "dial fastdial",() => Stargate.UI_Connect( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );
				}

				right.Add.Button( "Disconnect", "fill", () => Stargate.UI_Disconnect( Gate.NetworkIdent ) );
			}
		}

		public void SetGate( Stargate gate )
		{
			this.Gate = gate;
			ourAddress.Text = gate.Address;
		}

		public string CleanupAddress( string address )
		{
			address = address.RemoveBadCharacters();
			address = address.ToUpper();
			if ( address.Length > 6 )
			{
				address = address.Substring( 0, 6 );
			}

			var x = address.Where( ( c, i ) => i > 0 && address.Last() == address[i - 1] ).Cast<char?>().FirstOrDefault() != null;
			if ( x )
			{
				address = address.Remove( address.Length - 1 );
			}
			return address;
		}
	}
}

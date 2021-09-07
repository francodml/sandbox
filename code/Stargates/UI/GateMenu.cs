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
		public TextEntry ourAddress;
		private Label currentGlyph;

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
				ourAddress = content.Add.TextEntry( "" );
				ourAddress.AddClass( "addressinput" );
				ourAddress.Placeholder = "Gate Address";
				ourAddress.AddEventListener( "onchange", () => ourAddress.Text = CleanupAddress( ourAddress.Text ) );

				content.Add.Button( "Set", () =>
				{
					if ( ourAddress.Text.Length == 6 )
					{
						Sound.FromScreen( "stargate.ui.confirm" );
						Stargate.UI_SetGateAddress( Gate.NetworkIdent, ourAddress.Text.ToUpper() );
					}
				} );

				remoteAddress = content.Add.TextEntry( "" );
				remoteAddress.AddClass( "addressinput" );
				remoteAddress.Placeholder = "Remote Address";
				remoteAddress.AddEventListener( "onchange", () => remoteAddress.Text = CleanupAddress( remoteAddress.Text ) );

				content.Add.Button( "Connect", () => Stargate.UI_Connect( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );

				content.Add.Button( "Disconnect", () => Stargate.UI_Disconnect( Gate.NetworkIdent ) );

				content.Add.Button( "Animate", () => Stargate.UI_Animate( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );

				content.Add.Button( "Toggle Ring", () => Stargate.UI_ToggleRing( Gate.NetworkIdent ) );

				content.Add.Button( "Force Reset", () => Stargate.UI_ForceReset( Gate.NetworkIdent ) );

				currentGlyph = content.Add.Label( $"Current Glyph:", "glyphlabel" );
			}
		}

		public void SetGate( Stargate gate )
		{
			this.Gate = gate;
			gateaddress.Text = Gate.Address;
			ourAddress.Text = Gate.Address;
		}

		public string CleanupAddress( string address )
		{
			address = address.RemoveBadCharacters();
			address = address.ToUpper();
			if ( address.Length > 6 )
			{
				address.Substring( 0, 6 );
			}

			var x = address.Where( ( c, i ) => i > 0 && address.Last() == address[i-1] ).Cast<char?>().FirstOrDefault() != null;
			if ( x )
			{
				address = address.Remove( address.Length - 1 );
			}
			return address;
		}

		public override void Tick()
		{
			base.Tick();
			if ( Gate != null && currentGlyph.Text != Gate.CurrentRingSymbol )
			{
				currentGlyph.SetText( $"Current Glyph: {Gate.CurrentRingSymbol}" );
			}
		}

	}
}

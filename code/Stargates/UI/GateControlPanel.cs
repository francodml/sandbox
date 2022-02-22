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
		public TextEntry remoteAddress;

		protected bool MouseInside { get; set; }

		protected Angles lookatangles;

		public GateControlPanel( Stargate gate )
		{
			StyleSheet.Load( "Stargates/UI/GateControlPanel.scss" );
			Gate = gate;
			lookatangles = Local.Pawn.EyeRot.Angles();

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
					ourAddress = addressContainer.Add.TextEntry( Gate.Address );
					ourAddress.AddClass( "addressinput" );
					ourAddress.Placeholder = "Gate Address";
					ourAddress.AddEventListener( "onchange", () => ourAddress.Text = CleanupAddress( ourAddress.Text ) );

					Action action = () =>
						{
						   if ( ourAddress.Text.Length == 6 )
						   {
							   Sound.FromScreen( "stargate.ui.confirm" );
							   Stargate.UI_SetGateAddress( Gate.NetworkIdent, ourAddress.Text.ToUpper() );
						   }
						};
					ourAddress.AddEventListener( "onsubmit", action );

					Button button = addressContainer.Add.Button( "Set", action );
				}

				var namecontainer = left.AddChild<Panel>( "rowcontainer" );
				{
					var gateName = namecontainer.Add.TextEntry( Gate.Name );
					gateName.Placeholder = "Name";
					Action action = () =>
						{
						   Sound.FromScreen( "stargate.ui.confirm" );
						   Stargate.UI_SetGateName( Gate.NetworkIdent, gateName.Text );
						};
					gateName.AddEventListener( "onsubmit", action );

					namecontainer.Add.Button( "Set", action );
				}
			}

			var middle = content.AddChild<Panel>( "column middle" );
			{
				foreach (Stargate g in Entity.All.OfType<Stargate>().Where( x => x != Gate) )
				{
					middle.AddChild( new AddressBookEntry( g, this ) );
				}
			}

			var right = content.AddChild<Panel>( "column right" );
			{
				var addressContainer = right.AddChild<Panel>( "rowcontainer" );
				{
					remoteAddress = addressContainer.Add.TextEntry( "" );
					remoteAddress.AddClass( "addressinput" );
					remoteAddress.Placeholder = "Remote Address";
					remoteAddress.AddEventListener( "onchange", () => 
						remoteAddress.Text = CleanupAddress( remoteAddress.Text ) );
					remoteAddress.AddEventListener( "onsubmit", () => 
						Stargate.UI_Connect( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );

					addressContainer.Add.Button( "timer", "icon outline dial slowdial",() => Stargate.UI_Animate( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );
					addressContainer.Add.Button( "fast_forward", "icon outline dial fastdial",() => Stargate.UI_Connect( Gate.NetworkIdent, remoteAddress.Text.ToUpper() ) );
				}

				right.Add.Button( "Disconnect", "fill", () => Stargate.UI_Disconnect( Gate.NetworkIdent ) );
			}
		}

		public override void OnDeleted()
		{
			base.OnDeleted();
			Local.Pawn.Tags.Remove( "Stargate.PanelOpen" );
		}

		public override void Tick()
		{
			base.Tick();

			MouseInside = IsInside( Mouse.Position );

			Style.PointerEvents = MouseInside ? "all" : "visible";

			Style.Dirty();
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

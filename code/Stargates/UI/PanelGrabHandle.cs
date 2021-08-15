using System;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	[Library]
	public class PanelGrabHandle : Panel
	{
		Vector2 position;
		Panel MovePanel;
		public PanelGrabHandle( Panel parent, Vector2 initpos )
		{
			MovePanel = parent;
			position = initpos;
		}

		public override void OnButtonEvent( ButtonEvent e )
		{
			if ( e.Button == "mouseleft" )
			{
				SetMouseCapture( e.Pressed );
			}

			base.OnButtonEvent( e );
		}

		public override void Tick()
		{
			base.Tick();

			if ( HasMouseCapture )
			{
				position += Mouse.Delta;

				Log.Info( MovePanel.ElementName );
				
				MovePanel.Style.Left = Math.Clamp( position.x, 0, Screen.Width-800 );
				MovePanel.Style.Top = Math.Clamp( position.y, 0, Screen.Height-500 );
				MovePanel.Style.Dirty();
			}

		}
	}
}

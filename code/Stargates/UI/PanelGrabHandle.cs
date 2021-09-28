using System;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates.UI
{
	[Library]
	public class MouseDragPanel : Panel
	{

		public Panel Window => Parent;
		protected bool IsMoving { get; set; }
		protected Vector2 InitialPos { get; set; }

		protected override void OnMouseDown( MousePanelEvent e )
		{
			base.OnMouseDown( e );

			IsMoving = true;
			Style.Dirty();
		}

		protected override void OnMouseUp( MousePanelEvent e )
		{
			base.OnMouseUp( e );

			IsMoving = false;
		}
	}

	public class Titlebar : MouseDragPanel
	{
		protected override void OnMouseDown( MousePanelEvent e )
		{
			base.OnMouseDown( e );

			InitialPos = (Mouse.Position - new Vector2( Window.Box.Left, Window.Box.Top )) * ScaleFromScreen;
		}

		protected override void OnMouseMove( MousePanelEvent e )
		{
			base.OnMouseMove( e );

			if ( IsMoving )
			{
				var newPos = Mouse.Position * ScaleFromScreen - InitialPos;

				Window.Style.Left = Length.Pixels( newPos.x );
				Window.Style.Top = Length.Pixels( newPos.y );

				Window.Style.Dirty();
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using System.Numerics;

namespace winsandbox.Stargates
{
	public partial class DHDKey : ModelEntity, IUse
	{
		[Net] public string Glyph { get; set; } = "#";
		[Net] public bool Active { get; private set; } = false;
		public Color KeyColour => Active ? Color.Orange : Color.White;
		public DHD DHD => Parent as DHD;
		public DHDKey()
		{

		}
		public DHDKey( string model, Entity parent )
		{
			SetModel( model );
			SetParent( parent );
		}
		public bool IsUsable( Entity e ) => true;

		public async void SubmitDelay()
		{
			await Task.DelaySeconds( 0.6f );
			Active = !Active;
			RenderColor = KeyColour;
			await Task.DelaySeconds( 0.5f );
			DHD.KeyPress( Glyph, Active );
		}

		public bool OnUse( Entity actv )
		{
			if ( DHD.Stargate != null && DHD.Stargate.Busy && Glyph != "SUBMIT" )
				return false;
			if ( DHD.Stargate != null && DHD.Stargate.DenyDHDInput && Glyph != "#" & Glyph != "SUBMIT" )
				return false;
			if ( !Active )
				PlaySound( $"dhd.milkyway.{(Glyph == "SUBMIT" ? "submit" : "press")}" );
			if (Glyph == "SUBMIT" && !Active )
			{
				SubmitDelay();
				return false;
			}
			Active = !Active;
			RenderColor = KeyColour;
			DHD.KeyPress( Glyph, Active );
			return false;
		}

		public void Toggle(bool state )
		{
			Active = state;
			RenderColor = KeyColour;
		}

		[Event.Frame]
		public void OnFrame()
		{
			var bbox = CollisionBounds;
			DebugOverlay.Text(  Transform.TransformVector(CollisionBounds.Center), Glyph, KeyColour );
		}
	}
}

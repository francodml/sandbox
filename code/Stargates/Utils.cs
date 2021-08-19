using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace winsandbox.Stargates
{
	public static partial class Utils
	{
		public static List<char> AddressSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*!".ToList();
		public static Dictionary<Type, bool> IgnoredTypes = new()
		{
			[typeof(EventHorizon)] = true,
			[typeof(PickupTrigger)] = true
		};

		[ServerCmd("sg_dial_target", Help = "Dial the gate you're looking at")]
		public static void DialGate( string otheraddress )
		{
			var ply = ConsoleSystem.Caller.Pawn;

			var tr = Trace.Ray( ply.EyePos, ply.EyePos + ply.EyeRot.Forward * 100000 )
				.Ignore( ply )
				.WithTag("IsStargate")
				.Run();
			if ( tr.Hit && tr.Entity.IsValid() && tr.Entity is Stargate sg )
			{
				sg.Connect(otheraddress.ToUpperInvariant());
			}
		}

		[ServerCmd("sg_disconnect_target", Help = "Disconnects the gate you're looking at")]
		public static void DisconnectGate()
		{
			var ply = ConsoleSystem.Caller.Pawn;

			var tr = Trace.Ray( ply.EyePos, ply.EyePos + ply.EyeRot.Forward * 100000 )
				.Ignore( ply )
				.WithTag( "IsStargate" )
				.Run();
			if ( tr.Hit && tr.Entity.IsValid() && tr.Entity is Stargate sg )
			{
				sg.Disconnect();
			}
		}

		[ServerCmd("sg_animate_chevron")]
		public static void AnimateChevron( int index = 0 )
		{
			var ply = ConsoleSystem.Caller.Pawn;

			var tr = Trace.Ray( ply.EyePos, ply.EyePos + ply.EyeRot.Forward * 100000 )
				.Ignore( ply )
				.WithTag( "IsStargate" )
				.Run();
			if ( tr.Hit && tr.Entity.IsValid() && tr.Entity is Stargate sg )
			{
				sg.chevrons[index].Animate();
			}
		}

		[ServerCmd( "sg_rotate_ring" )]
		public static void RotateRing( float degrees = 0 )
		{
			var ply = ConsoleSystem.Caller.Pawn;

			var tr = Trace.Ray( ply.EyePos, ply.EyePos + ply.EyeRot.Forward * 100000 )
				.Ignore( ply )
				.WithTag( "IsStargate" )
				.Run();
			if ( tr.Hit && tr.Entity.IsValid() && tr.Entity is Stargate sg )
			{
				sg.RotateRing( degrees );
			}
		}
	}
}

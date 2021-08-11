using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace winsandbox.Stargates
{
    public static class Utils
    {
        public static List<char> AddressSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToList();

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
    }
}

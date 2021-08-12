using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace winsandbox.Stargates
{
	public static class Utils
	{
		public static List<char> AddressSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToList();
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

		[ServerCmd("rotateplayer")]
		public static void RotatePlayer()
		{
			var ply = ConsoleSystem.Caller.Pawn as AnimEntity;
			var newRot = new Vector3( 0, 0, 0 );
			ply.SetAnimLookAt( "aim_eyes", newRot );
			ply.SetAnimLookAt( "aim_head", newRot );
			ply.SetAnimLookAt( "aim_body", newRot );

			ply.SetAnimFloat( "aim_eyes_weight", 1.0f );
			ply.SetAnimFloat( "aim_head_weight", 1.0f );
			ply.SetAnimFloat( "aim_body_weight", 1.0f );

			ply.EyeRot = new Angles( 0, 0, 0 ).ToRotation();
		}

		[ClientCmd("cl_rotateplayer")]
		public static void clrotateplayer()
		{
			var ply = ConsoleSystem.Caller.Pawn as AnimEntity;
			ply.EyeRot = new Angles( 0, 0, 0 ).ToRotation();
			var newRot = new Vector3( 10000, 1000, 1000 );
			ply.SetAnimLookAt( "aim_eyes", newRot );
			ply.SetAnimLookAt( "aim_head", newRot );
			ply.SetAnimLookAt( "aim_body", newRot );

			ply.SetAnimFloat( "aim_eyes_weight", 1.0f );
			ply.SetAnimFloat( "aim_head_weight", 1.0f );
			ply.SetAnimFloat( "aim_body_weight", 1.0f );
		}
    }
}

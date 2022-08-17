using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	public enum ConnectionType
	{
		None,
		Incoming,
		Outgoing
	}

	public enum GateState
	{
		Idle,
		Dialling,
		Open,
		Closing
	}
	public static partial class Utils
	{
		public static List<char> AddressSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789*@".ToList();
		public static Dictionary<Type, bool> IgnoredTypes = new()
		{
			[typeof(EventHorizon)] = true,
			[typeof(PickupTrigger)] = true
		};

		[ServerCmd("sg_dial_target", Help = "Dial the gate you're looking at")]
		public static void DialGate( string otheraddress )
		{
			var ply = ConsoleSystem.Caller.Pawn;

			var tr = Trace.Ray( ply.EyePosition, ply.EyePosition + ply.EyeRotation.Forward * 100000 )
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

			var tr = Trace.Ray( ply.EyePosition, ply.EyePosition + ply.EyeRotation.Forward * 100000 )
				.Ignore( ply )
				.WithTag( "IsStargate" )
				.Run();
			if ( tr.Hit && tr.Entity.IsValid() && tr.Entity is Stargate sg )
			{
				sg.Disconnect();
			}
		}

		[ServerCmd( "sg_rotate_ring" )]
		public static void RotateRing( string symbol )
		{
			var ply = ConsoleSystem.Caller.Pawn;

			var tr = Trace.Ray( ply.EyePosition, ply.EyePosition + ply.EyeRotation.Forward * 100000 )
				.Ignore( ply )
				.WithTag( "IsStargate" )
				.Run();
			if ( tr.Hit && tr.Entity.IsValid() && tr.Entity is Stargate sg )
			{
				sg.RotateRingToSymbol(symbol.ToUpper());
			}
		}

		[ClientCmd]
		public static void OutputRingTemplate()
		{
			string result = "";
			float nine = 9.235f;

			for (int i = 0; i < 39; i++ )
			{
				var suffix = i % 2 == 1 ? "\n" : "";
				var template = $"[\"\"] = {nine * i}f, {suffix}";
				result += template;
			}

			Log.Info( result );

			Clipboard.SetText( result );
		}
	}
}

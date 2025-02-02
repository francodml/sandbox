﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.sandbox_extensions
{
	public static partial class UndoSystem
	{
		static Dictionary<Client, List<int>> UndoList = new();

		public static void AddUndoEntry(Client Owner, int NetworkIdent)
		{

			bool Exists = UndoList.TryGetValue( Owner, out var PlayerList );
			if (!Exists)
			{
				UndoList.Add( Owner, new List<int> { NetworkIdent } );
				return;
			}

			PlayerList.Add( NetworkIdent );
		}

		[ServerCmd]
		public static void wsb_undo()
		{
			var owner = ConsoleSystem.Caller;

			bool Exists = UndoList.TryGetValue( owner, out var PlayerList );
			if ( !Exists || PlayerList.Count == 0 ) return;

			var ent = Entity.FindByIndex( PlayerList.Last() );

			if ( ent == null )
			{
				PlayerList.RemoveAt( PlayerList.Count - 1 );
				wsb_undo();
				return;
			}

			ent.Delete();
			PlayerList.RemoveAt( PlayerList.Count-1 );
			Log.Info( $"Removed entity {ent.NetworkIdent} ({ent.GetType()})" );
			undo_client(To.Single(owner));
		}

		[ClientRpc]
		public static void undo_client()
		{
			Sound.FromScreen( "ui.undo" );
		}

		[ServerCmd]
		public static void wsb_undo_list()
		{
			var owner = ConsoleSystem.Caller;

			bool Exists = UndoList.TryGetValue( owner, out var PlayerList );
			if ( !Exists )
			{
				Log.Info( "No undo list" );
				return;
			}

			foreach ( int ID in PlayerList )
			{
				Log.Info( ID );
			}

		}
	}
}

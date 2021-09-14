using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using System.ComponentModel;
using System.Threading;

namespace winsandbox.Stargates
{
	public partial class Stargate
	{
		public bool DenyDHDInput => (!string.IsNullOrEmpty(OtherAddress) && OtherAddress.Length == 8) | ShouldDenyDHDInput;
		[Editor, Category( "Stargate" )]
		[Reset( false )] public bool ShouldDenyDHDInput { get; private set; } = false;
		[Reset( 0 ), Category( "Stargate" )] public int currentChevron { get; private set; } = 0;
		[Reset(false)] public bool Dialling { get; private set; } = false;
		private bool locking = false;
		private bool doingStuff;

		private Dictionary<string, int> AddressIndexMap;
		[Reset( false) ] private bool shouldLockOnStop { get; set; }
		[Reset( false )] private bool shouldConnectOnStop { get; set; }
		[Reset( false )] private bool shouldTryConnect { get; set; }

		public void EncodeChevron()
		{
			if ( !IsServer )
				return;
			if ( DHD != null )
				DHD.SetKey( CurrentRingSymbol, true );
			if ( (currentChevron == 9 || currentChevron == 6) && !chevrons[6].Engaged)
			{
				LockChevron(true);
				return;
			}
			chevrons[6].OtherChevron = chevrons[currentChevron];
			chevrons[6].Animate();
			currentChevron++;
			if ( OtherAddress.Length > 7 && currentChevron == 6 )
				currentChevron++;
		}

		public void LockChevron( bool autoconnect = false )
		{
			var chevron = chevrons[6];
			chevron.IsFinalLock = true;
			var other = FindGate();
			if ( other == null )
				chevron.FailedLock = true;
			else
			{
				other.SetChevrons( true );
			}
			chevron.Animate(true);
			if (autoconnect)
				currentChevron = 0;
			Dialling = false;
			locking = autoconnect;
		}

		public void DHDKeypress( string glyph, bool state )
		{
			if ( AddressIndexMap == null )
				AddressIndexMap = new();
			switch ( glyph )
			{
				case "SUBMIT":
					if ( shouldLockOnStop )
					{
						shouldConnectOnStop = true;
						break;
					}
					if ( Connection != ConnectionType.None )
					{
						Disconnect();
						DHD.Reset();
						break;
					}
					Connect();
					currentChevron = 0;
					break;
				case var value when value == PointOfOrigin:
					if ( state )
					{
						ring.RotateToSymbol( PointOfOrigin );
						ShouldDenyDHDInput = true;
						shouldLockOnStop = true;
					}
					else if ( chevrons[6].Engaged )
					{
						chevrons[6].Toggle( false, false );
						ShouldDenyDHDInput = false;
						ring.Start();
					}
					break;
				default:
					if ( state )
					{
						if ( !ring.Rotating )
							ring.Start();
						if ( OtherAddress.Length == 8 )
							break;
						OtherAddress += glyph;
						AddressIndexMap[glyph] = AddressIndexMap.Count;
						chevrons[currentChevron].Toggle( silent: false );
						currentChevron++;
						if ( OtherAddress.Length == 6 && currentChevron == 6 )
							currentChevron++;
					} else
					{
						if ( !AddressIndexMap.ContainsKey( glyph ) )
							break;
						var index = AddressIndexMap[glyph];
						currentChevron--;
						if ( OtherAddress.Length == 6 && currentChevron == 6 )
							currentChevron--;
						chevrons[currentChevron].Toggle( silent: false );
						OtherAddress = OtherAddress.Remove( index, 1 );
						if ( OtherAddress.Length == 0 )
						{
							Reset();
							break;
						}
						RegenerateIndexMap();
					}
					break;
			}
		}

		private void RegenerateIndexMap()
		{
			AddressIndexMap = new();
			for (int i = 0; i < OtherAddress.Length; i++ )
			{
				AddressIndexMap[OtherAddress[i].ToString()] = i;
			}
		}

		public async void EncodeDelay()
		{
			doingStuff = true;
			await Task.DelaySeconds( 0.6f );
			EncodeChevron();
			doingStuff = false;
		}

		public async void LockDelay()
		{
			shouldLockOnStop = false;
			await Task.DelaySeconds( 0.6f );
			LockChevron();
			shouldTryConnect = true;
		}

		[Event.Tick.Server]
		public void Tick()
		{
			if ( Dialling && !string.IsNullOrEmpty(OtherAddress) )
			{
				var desiredSymbol = currentChevron < OtherAddress.Length ? OtherAddress[currentChevron].ToString() : "#";
				if ( !chevrons[6].Continue )
					return;
				if ( !ring.Rotating && ring.CurrentSymbol != desiredSymbol )
					ring.RotateToSymbol( desiredSymbol );
				if ( !ring.Rotating && ring.CurrentSymbol == desiredSymbol && !chevrons[currentChevron].Engaged && !doingStuff )
				{
					EncodeDelay();
				}
			}

			if ( currentChevron > 8 )
				currentChevron = 6;

			if ( locking && chevrons[6].Continue )
			{
				locking = false;
				Connect();
			}

			if ( !ring.Rotating && shouldLockOnStop )
			{
				LockDelay();
			}

			if ( shouldConnectOnStop && shouldTryConnect && chevrons[6].Continue )
			{
				shouldConnectOnStop = false;
				shouldTryConnect = false;
				Connect();
			}
		}

	}
}

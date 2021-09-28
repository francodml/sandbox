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
		[Reset(false)] public bool DoSlowDial { get; private set; } = false;
		private bool locking = false;
		private bool doingStuff;

		private Dictionary<string, int> AddressIndexMap;
		[Reset( false )] public bool shouldLockOnStop { get; private set; }
		[Reset( false )] public bool shouldConnectOnStop { get; private set; }

		public void EncodeChevron()
		{
			if ( !IsServer )
				return;
			if ( (currentChevron == 9 || currentChevron == 6) && !TopChevron.Engaged)
			{
				LockChevron(true);
				return;
			}
			TopChevron.OtherChevron = chevrons[currentChevron];
			TopChevron.Animate();
			currentChevron++;
		}

		public void LockChevron( bool autoconnect = false )
		{
			var chevron = TopChevron;
			chevron.IsFinalLock = true;
			var other = FindGate();
			OtherGate = other;
			if ( other == null )
				chevron.FailedLock = true;
			else
			{
				other.SetChevrons( true, true );
			}
			chevron.Animate(true);
			if (autoconnect)
				currentChevron = 0;
			DoSlowDial = false;
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
						break;
					}
					Connect();
					currentChevron = 0;
					break;
				case var value when value == PointOfOrigin:
					if ( state )
					{
						ring.Stop();
						ShouldDenyDHDInput = true;
						shouldLockOnStop = true;
					}
					else if ( !state )
					{
						TopChevron.Toggle( false, false );
						if ( OtherGate != null && OtherGate.IsValid )
							OtherGate.SetChevrons( false, true );
						ShouldDenyDHDInput = false;
						ring.Start();
					}
					break;
				default:
					if ( state )
					{
						State = GateState.Dialling;
						if ( !ring.Rotating )
							ring.Start();
						if ( OtherAddress.Length == 8 )
							break;
						OtherAddress += glyph;
						AddressIndexMap[glyph] = AddressIndexMap.Count;
						chevrons[currentChevron].Toggle( silent: false );
						currentChevron++;
					} else
					{
						if ( !AddressIndexMap.ContainsKey( glyph ) )
							break;
						var index = AddressIndexMap[glyph];
						currentChevron--;
						chevrons[currentChevron].Toggle( silent: false );
						OtherAddress = OtherAddress.Remove( index, 1 );
						if ( OtherAddress.Length == 0 )
						{
							State = GateState.Idle;
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
			if ( DHD != null )
				DHD.SetKey( CurrentRingSymbol, true );
			doingStuff = false;
		}

		public async void LockDelay()
		{
			shouldLockOnStop = false;
			await Task.DelaySeconds( 0.6f );
			LockChevron(shouldConnectOnStop);
		}

		public async void FastDial( string address )
		{
			if ( Busy | State == GateState.Dialling )
				return;

			State = GateState.Dialling;
			OtherAddress = address;
			ring.Start();
			shouldLockOnStop = true;
			shouldConnectOnStop = true;
			await Task.DelaySeconds( 0.5f );
			int j = 0;
			for (int i = 0; i < OtherAddress.Length; i++ )
			{
				if ( this == null | !this.IsValid )
					return;
				if ( State != GateState.Dialling )
					break;
				chevrons[j].Toggle( true );
				if ( DHD != null )
					DHD.SetKey( OtherAddress[i].ToString(), true );
				PlaySound( "stargates.milkyway.chevron.open" );
				await Task.DelaySeconds( 0.5f + Rand.Float( 0.5f ) );
				j++;
				if ( j == 6 )
					j++;
			}
			if ( this == null | !this.IsValid )
				return;
			if ( State != GateState.Dialling )
				return;
			if ( DHD != null )
				DHD.SetKey( PointOfOrigin, true );
			ring.Stop();
		}

		public void SlowDial( string address )
		{
			if ( Busy | State == GateState.Dialling )
				return;
			State = GateState.Dialling;
			OtherAddress = address;
			DoSlowDial = true;
		}

		[Event.Tick.Server]
		public void Tick()
		{
			if ( DoSlowDial && !string.IsNullOrEmpty(OtherAddress) )
			{
				var desiredSymbol = currentChevron < OtherAddress.Length ? OtherAddress[currentChevron].ToString() : "#";
				if ( !TopChevron.Continue )
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

			if ( locking && TopChevron.Continue )
			{
				locking = false;
				Connect();
			}

			if ( !ring.Rotating && shouldLockOnStop )
			{
				LockDelay();
			}
		}

	}
}

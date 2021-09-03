using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	public partial class Ring : ModelEntity
	{
		public bool Rotating { get; private set; } = false;
		public float TopSpeed { get; set; } = 0.8f;
		public float Acceleration { get; set; } = 0.02f;
		public bool Direction { get; set; } = false;
		public string RequestedSymbol { get; set; }
		[Net] public string CurrentSymbol => SymbolAngles.ElementAt( currentSymbol ).Key;

		private float speed = 0f;
		private float distanceTravelled = 0f;

		private bool ShouldRotateRing = false;

		private Sound spinSound;
		private Sound startSound;

		[Net] private int currentSymbol { get; set; } = 0;

		private bool symbolTicked;
		private bool shouldPlayLoop;

		private static Dictionary<string, float> SymbolAngles = new()
		{
			["#"] = 0.0f,			["0"] = 9.235f,
			["J"] = 18.47f,			["K"] = 27.705f,
			["N"] = 36.94f,			["T"] = 46.175f,
			["R"] = 55.41f,			["3"] = 64.645f,
			["M"] = 73.88f,			["B"] = 83.115f,
			["Z"] = 92.35f,			["X"] = 101.585f,
			["*"] = 110.81999f,		["H"] = 120.05499f,
			["6"] = 129.29f,		["9"] = 138.525f,
			["I"] = 147.76f,		["G"] = 156.995f,
			["P"] = 166.23f,		["L"] = 175.465f,
			["@"] = 184.7f,			["Q"] = 193.935f,
			["F"] = 203.17f,		["S"] = 212.405f,
			["1"] = 221.63998f,		["E"] = 230.87498f,
			["4"] = 240.10999f,		["A"] = 249.34499f,
			["U"] = 258.58f,		["8"] = 267.815f,
			["5"] = 277.05f,		["O"] = 286.285f,
			["C"] = 295.52f,		["W"] = 304.75497f,
			["7"] = 313.99f,		["2"] = 323.22498f,
			["Y"] = 332.46f,		["V"] = 341.69498f,
			["D"] = 350.93f,		
		};


		public override void Spawn()
		{
			base.Spawn();
			SetModel( "models/stargates/ring.vmdl" );
		}
		protected override void OnDestroy()
		{
			base.OnDestroy();
			spinSound.Stop();
		}

		public async void Start()
		{
			if ( Rotating )
				return;
			ShouldRotateRing = true;
			Rotating = true;
			Direction = !Direction;
			startSound = PlaySound( "stargates.milkyway.ring.start" );
			shouldPlayLoop = true;
			await Task.DelaySeconds( 4.0f );
			if (shouldPlayLoop)
				spinSound = PlaySound( "stargates.milkyway.ring.loop" );
		}

		public void Stop()
		{
			if ( !Rotating )
				return;
			ShouldRotateRing = false;
			RequestedSymbol = null;
			distanceTravelled = 0;
		}

		public void Toggle()
		{
			if (Rotating)
			{
				Stop();
			} 
			else
			{
				Start();
			}
		}

		public void RotateToSymbol( string symbol )
		{
			RequestedSymbol = symbol;
			Start();
		}

		/*public string GetCurrentSymbol()
		{

		}*/

		[Event.Tick.Server]
		public void Tick()
		{
			if ( speed != (ShouldRotateRing ? TopSpeed : 0) )
			{
				speed = Math.Clamp( speed + (Acceleration * (ShouldRotateRing ? 1 : -1)), 0f, TopSpeed );
			}
			LocalRotation = LocalRotation.RotateAroundAxis( LocalRotation.Forward, speed * (Direction ? 1 : -1) );
			distanceTravelled += speed;

			if ( !ShouldRotateRing && speed == 0 && Rotating )
			{
				PlaySound( "stargates.milkyway.ring.stop" );
				spinSound.Stop();
				startSound.Stop();
				shouldPlayLoop = false;
				Rotating = false;
			}

			if ( !string.IsNullOrEmpty( RequestedSymbol ) )
			{
				float stopCalc = (SymbolAngles[RequestedSymbol] - LocalRotation.Roll().NormalizeDegrees());
				float tolerance = ((speed / Acceleration) * speed / 2) * (Direction ? 1 : -1);

				if ( ShouldRotateRing && distanceTravelled > 184.7f )
				{
					if ( SymbolAngles[RequestedSymbol] < tolerance )
					{
						if ( stopCalc <= (-360 + tolerance) )
							Stop();
					}
					
					else if ( RequestedSymbol == "D" && !Direction /*&& SymbolAngles[RequestedSymbol] < -tolerance + 360 && !Direction*/)
					{
						if ( stopCalc > 360 - -tolerance)
							Stop();
					}

					else if ( MathF.Sign( stopCalc ) == -1 ? stopCalc >= tolerance : stopCalc <= tolerance )
						Stop();
				}
			}


			var symbolProgress = Math.Round( LocalRotation.Roll().NormalizeDegrees().UnsignedMod( 9.235f ) );

			if ( Rotating && symbolProgress == 4  && !symbolTicked)
			{
				symbolTicked = true;
				if ( currentSymbol == 38 && Direction )
				{
					currentSymbol = 0;
					return;
				}
				if ( currentSymbol == 0 && !Direction )
				{
					currentSymbol = 38;
					return;
				}
				currentSymbol += (Direction ? 1 : -1);
			} 
			else if( (symbolProgress < 4 || !Rotating) && symbolTicked)
			{
				symbolTicked = false;
			}
		}

	}
}

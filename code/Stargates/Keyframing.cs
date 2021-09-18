using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.Stargates
{
	struct Keyframe
	{
		public float Time;
		public float Value;
		public EasingFunction Easing;

		public Keyframe( float time, float value, EasingFunction easing )
		{
			Time = time;
			Value = value;
			Easing = easing;
		}

		public static implicit operator float(Keyframe x) => x.Value;
	}

	enum EasingFunction
	{
		Linear,
		ExpoOut,
		ExpoIn,
		Expo,
		QuadIn,
		QuadOut,
		Quad,
		CubicIn,
		CubicOut,
		Cubic,
		Quartic,
		Quint,
		Circular,
		Sine,
		Hold
	}

	class Timeline
	{
		public List<Keyframe> Keyframes;
		public int CurrentKeyframe;
		public int NextKeyFrame;
		public Keyframe Current => Keyframes[CurrentKeyframe];
		public Keyframe Next => Keyframes[NextKeyFrame];
		public bool Finished => NextKeyFrame == Keyframes.Count - 1;

		public Timeline()
		{
			Keyframes = new();
			CurrentKeyframe = 0;
			NextKeyFrame = 1;
		}

		void Advance()
		{
			if ( Finished )
				return;
			CurrentKeyframe++;
			NextKeyFrame++;
		}

		void Reset()
		{
			CurrentKeyframe = 0;
			NextKeyFrame = 1;
		}

		public float GetTimeInterval(float time)
		{
			var oldRange = Next.Time - Current.Time;
			return Math.Clamp((time - Current.Time) / oldRange, 0.0f, 1.0f);
		}
		public float GetValue( float time )
		{
			if ( time <= Keyframes[0].Time )
				Reset();
			if (GetTimeInterval(time) >= 1 )
			{
				Advance();
			}
			return MathX.LerpTo( Current, Next, Ease(GetTimeInterval( time ), Next.Easing) );
		}

		private float Ease( float x, EasingFunction easing )
		{
			switch ( easing )
			{
				case EasingFunction.Linear:
					return x;
				case EasingFunction.ExpoOut:
					return x == 1 ? 1 : 1 - MathF.Pow( 2, -10 * x );
				case EasingFunction.Quint:
					return x < 0.5
					  ? (1 - MathF.Sqrt( 1 - MathF.Pow( 2 * x, 2 ) )) / 2
					  : (MathF.Sqrt( 1 - MathF.Pow( -2 * x + 2, 2 ) ) + 1) / 2;
				default:
					return x;
			}
		}

		public Timeline WithKeyframe( Keyframe k )
		{
			Keyframes.Add( k );

			Keyframes = Keyframes.OrderBy( x => x.Time).ToList();

			return this;
		}

		public Timeline WithKeyframe( float time, float value = 0.0f, EasingFunction easing = EasingFunction.Linear )
		{
			Keyframe k = new( time, value, easing );

			return WithKeyframe( k );
		}
	}
}

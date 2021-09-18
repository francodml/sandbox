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
		Exponential,
		Quadratic,
		Cubic,
		Quartic,
		Quintic,
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

		private float Ease( float v, EasingFunction easing )
		{
			switch ( easing )
			{
				case EasingFunction.Linear:
					return v;
				case EasingFunction.Exponential:
					return Easing.EaseInOut(v);
				default:
					return v;
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

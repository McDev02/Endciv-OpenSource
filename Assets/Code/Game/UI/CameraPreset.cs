using System;
using UnityEngine;
namespace Endciv
{
	public class CameraPreset : ScriptableObject
	{
		public CameraProperty PosX, PosY;
		public float sprintPanFactor = 1;
		public CameraPropertyRelative Pitch;
		public MinMax minPitchByDistance;
		public CameraProperty Yaw;
		public CameraProperty Zoom;

		[Serializable]
		public class CameraProperty
		{
			public float Speed, Adaption, Min, Max;
			float diff, diffInv;
			[NonSerialized]
			public float Current, Target;
			//Target value mapped to 0-1
			public float TargetRelative { get { return (Target - Min) * diffInv; } }
			//Current value mapped to 0-1
			public float CurrentRelative { get { return (Current - Min) * diffInv; } }

			internal void Setup()
			{
				diff = Max - Min;
				diffInv = 1f / diff;
			}
			internal void AddTarget(float v, bool clamp = true)
			{
				if (clamp)
					Target = Mathf.Clamp(Target + v * Speed, Min, Max);
				else
					Target += v * Speed;
			}

			internal void SetRelative(float v, bool force = false)
			{
				Target = Mathf.Lerp(Min, Max, v);
				if (force)
					Current = Target;
			}

			internal void ApplyCurrent(float d)
			{
				Current = Mathf.Lerp(Current, Target, Adaption * d);
			}
		}
		[Serializable]
		public class CameraPropertyRelative
		{
			public float Speed, Adaption, Min, Max;

			public float Current { get { return CurrentRelative * (Max - Min) + Min; } }
			public float Target { get { return TargetRelative * (Max - Min) + Min; } }
			[NonSerialized]
			public float TargetRelative, CurrentRelative;

			internal void Setup()
			{
			}
			internal void AddTarget(float v, bool clamp = true)
			{
				if (clamp)
					TargetRelative = Mathf.Clamp01(TargetRelative + v * Speed);
				else
					TargetRelative += v * Speed;
			}

			internal void SetRelative(float v, bool force = false)
			{
				TargetRelative = Mathf.Clamp01(v);
				if (force)
					CurrentRelative = TargetRelative;
			}

			internal void SetAbsolute(float v, bool force = false)
			{
				TargetRelative = (v - Min) / (Max - Min);
				if (force)
					CurrentRelative = TargetRelative;
			}

			internal void ApplyCurrent(float d)
			{
				CurrentRelative = Mathf.Lerp(CurrentRelative, TargetRelative, Adaption * d);
			}
		}
	}
}
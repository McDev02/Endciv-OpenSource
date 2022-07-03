using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public class SoundPool : ScriptableObject
	{
		[Serializable]
		public struct SoundPoolEntry
		{
			public string Key;
			public AudioClip[] Sound;
		}

		[SerializeField]
		public List<SoundPoolEntry> sounds;
	}
}
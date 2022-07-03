using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
	public class BlendedTimeline<T> where T : ScriptableObject, IBlendedNode<T>
	{
		List<T> nodes = new List<T>();
		float totalLengthf;
		private T blendedNode;
		public T BlendedNode
		{
			get { return blendedNode; }
			set { blendedNode = value; }
		}
		float sampleOffset;
		float sampleOffsetAbsolute;
		float fadeLength;
		float fadeLengthInv;
		float fadeLengthHalf;

		public BlendedTimeline(float sampleOffset)
		{
			BlendedNode = ScriptableObject.CreateInstance<T>();
			this.sampleOffset = sampleOffset;
			fadeLength = float.MaxValue;
		}

		public void AddNode(T node)
		{
			if (!nodes.Contains(node))
			{
				nodes.Add(node);
				node.StartLength = totalLengthf;
				totalLengthf += node.Length;
				if (fadeLength > node.Length)
				{
					fadeLength = node.Length;
					fadeLengthInv = 1f / fadeLength;
					fadeLengthHalf = fadeLength / 2f;

					for (int i = 0; i < nodes.Count; i++)
					{
						var n = nodes[i];
						n.StartFade = n.Length - fadeLengthHalf;
						n.EndFade = fadeLengthHalf;
					}
				}
				else
				{
					node.StartFade = node.Length - fadeLengthHalf;
					node.EndFade = fadeLengthHalf;
				}
				if (nodes.Count == 1)
					sampleOffsetAbsolute = node.Length * sampleOffset;

			}
		}

		internal float GetEndFadeTime(int v)
		{
			var n = nodes[v];
			return (n.StartLength + n.EndFade + totalLengthf - sampleOffsetAbsolute) % totalLengthf;
		}

		internal float GetStartFadeTime(int v)
		{
			var n = nodes[v];
			return (n.StartLength + n.StartFade + totalLengthf - sampleOffsetAbsolute) % totalLengthf;
		}

		/// <summary>
		/// Time from 0 to totalLength
		/// </summary>
		public T GetBlendedNode(float time)
		{
			var pretick = time;
			//Normalize Time
			time = (time + sampleOffsetAbsolute + totalLengthf) % totalLengthf;
			if (float.IsNaN(time))
				Debug.LogError($"Tick is Nan pretick:{pretick} totalLength:{ totalLengthf.ToString()}");
			var baseID = GetIDForBlending(time);
			var nextID = (baseID + 1) % nodes.Count;

			var nodeBase = nodes[baseID];
			var nodeNext = nodes[nextID];

			float t = (time - nodeBase.StartLength + totalLengthf) % totalLengthf;  //Normalize for loop
			t = Mathf.Clamp01((t - nodeBase.StartFade) * fadeLengthInv);

			//Debug.Log($"Blend Timeline {t.ToString("0.000")} From {nodeBase.name} To {nodeNext.name}");
			nodeBase.Blend(nodeNext, t, ref blendedNode);

			return blendedNode;
		}

		public int GetID(float time)
		{
			float tLength = 0;
			time = (time + sampleOffsetAbsolute + totalLengthf) % totalLengthf;
			for (int i = 0; i < nodes.Count; i++)
			{
				tLength += nodes[i].Length;
				if (time < tLength)
					return i;
			}
			Debug.LogError($"ID out of range tLengt: {tLength} - Time:{time}");
			return 0;
		}

		public int GetIDForBlending(float time)
		{
			float tLength = 0;
			time = (time + totalLengthf) % totalLengthf;

			for (int i = 0; i < nodes.Count; i++)
			{
				if (time < tLength + nodes[i].EndFade)
					return (i - 1 + nodes.Count) % nodes.Count;
				tLength += nodes[i].Length;
			}
			//return last else which completes the loop on the right end
			return nodes.Count - 1;
		}

		public T GetNode(float time)
		{
			return nodes[GetID(time)];
		}
	}

	public interface IBlendedNode<T>
	{
		string Name { get; }
		float Length { get; }
		//At with point the node starts in the timeline
		float StartLength { get; set; }
		//At with point we begin to fade out (should be on the right side)  Values between 0 and Length
		float StartFade { get; set; }
		//At with point we end fade in (should be on the left side) Values between 0 and Length
		float EndFade { get; set; }
		void Blend(T BlendData, float value);
		void Blend(T blendPreset, float value, ref T result);
	}
}

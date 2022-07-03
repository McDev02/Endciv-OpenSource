using System;
using System.Collections.Generic;
using UnityEngine;
namespace Endciv
{
	public class WaypointPath : MonoBehaviour
	{
		public Vector3[] points;
		public Vector3 GetWorldPoint(int id) { return transform.localToWorldMatrix.MultiplyPoint(points[id]); }
	}
}
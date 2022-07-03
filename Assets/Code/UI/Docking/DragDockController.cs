using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Endciv
{
	[RequireComponent(typeof(RectTransform))]
	public class DragDockController : MonoBehaviour
	{
		private Vector2 rootPosition;
		private Vector2[] closestPosition;
		private float[] closestDistances;
		private bool[] closestFound;
		private Vector2[] points, oPoints, dir;
		private DraggableElement draggedElement;
		private List<DockableElement> dockableElements;

		public float padding = 0;
		public float tolerance = 5;

		private GameInputManager inputManager;

		// Use this for initialization
		void Awake()
		{
			inputManager = Main.Instance.gameInputManager;

			closestPosition = new Vector2[4];
			closestDistances = new float[4];
			closestFound = new bool[4];
			points = new Vector2[4];
			oPoints = new Vector2[4];
			dir = new Vector2[]
			{
				Vector2.left,
				Vector2.up,
				Vector2.right,
				Vector2.down
			};
			dockableElements = new List<DockableElement>();

			inputManager.OnCanvasScaleChanged -= UpdateAllWindows;
			inputManager.OnCanvasScaleChanged += UpdateAllWindows;

			Main.Instance.graphicsManager.OnScreenResolutionChanged -= UpdateAllWindows;
			Main.Instance.graphicsManager.OnScreenResolutionChanged += UpdateAllWindows;
			SceneManager.sceneLoaded -= UnloadDockableWindows;
			SceneManager.sceneLoaded += UnloadDockableWindows;
		}

		private void Update()
		{
			if (draggedElement != null)
				OnUpdatePosition(draggedElement, !Input.GetKey(KeyCode.LeftShift));
		}

		void UpdateAllWindows()
		{
			for (int i = 0; i < dockableElements.Count; i++)
			{
				var obj = dockableElements[i];
				var drag = obj.GetComponentInChildren<DraggableElement>();
				if (drag != null)
					OnUpdatePosition(drag, true);
			}
		}

		internal void BeginDrag(DraggableElement d, PointerEventData e)
		{
			if (draggedElement != null)
				Debug.LogError("It should not occur that a new pointer is being registered while a draggable Element is already active.");
			else
			{
				d.dragRelativePosition = d.parentObject.anchoredPosition - e.position * inputManager.UIScaleInv;
				rootPosition = e.position;
				draggedElement = d;
				d.parentObject.SetAsLastSibling();
			}
		}

		internal void RegisterDockableElement(DockableElement d)
		{
			if (!dockableElements.Contains(d))
			{
				dockableElements.Add(d);
				d.transform.SetParent(transform, true);
				d.transform.localScale = Vector3.one;
			}
		}

		internal void DeregisterDockableElement(DockableElement d)
		{
			dockableElements.Remove(d);
		}

		internal void StopDrag(DraggableElement d)
		{
			draggedElement = null;
		}

		public void OnUpdatePosition(DraggableElement d, bool docking)
		{
			Vector2 pos;
			if (d.lastEventData == null)
				pos = d.parentObject.anchoredPosition;
			else
			{
				pos = d.lastEventData.position * inputManager.UIScaleInv;
				pos += d.dragRelativePosition;
			}
			var size = d.rectBounds.rect.size;
			pos.x = Mathf.Clamp(pos.x, 0, inputManager.scaledScreenBounds.x - size.x);
			pos.y = Mathf.Clamp(pos.y, 0, inputManager.scaledScreenBounds.y - size.y);

			if (docking)
			{
				UpdateDocking(d.dockableElement, ref pos);
				pos.x = Mathf.Clamp(pos.x, 0, inputManager.scaledScreenBounds.x - size.x);
				pos.y = Mathf.Clamp(pos.y, 0, inputManager.scaledScreenBounds.y - size.y);
			}

			//Snap to 2 pixels due to UI scale
			pos.x = (int)(pos.x * 0.5f + 0.5f) * 2;
			pos.y = (int)(pos.y * 0.5f + 0.5f) * 2;

			d.parentObject.anchoredPosition = pos;
		}

		//This is how a rectangle is represented, here are the indecies of edges and corners
		//		1---1---2		
		//		|		|
		//		0		2
		//		|		|
		//		0---3---3
		internal void UpdateDocking(DockableElement d, ref Vector2 pos)
		{
			points[0] = pos;
			points[2] = pos + d.rect.rect.size;
			points[1] = new Vector2(points[0].x, points[2].y);
			points[3] = new Vector2(points[2].x, points[0].y);

			for (int i = 0; i < 4; i++)
			{
				closestFound[i] = false;
				closestPosition[i] = Vector2.zero;
				closestDistances[i] = float.MaxValue;
			}
			if (d == null) return;
			Vector2 s;
			for (int j = 0; j < dockableElements.Count; j++)
			{
				var other = dockableElements[j];
				if (other == d) continue;

				oPoints[0] = other.Min;
				oPoints[2] = other.Max;
				oPoints[1] = new Vector2(oPoints[0].x, oPoints[2].y);
				oPoints[3] = new Vector2(oPoints[2].x, oPoints[0].y);

				for (int i = 0; i < 4; i++)
				{
					//Opposite ID
					var o = (i + 2) % 4;
					var inext = (i + 1) % 4;

					Vector2 off = dir[i] * tolerance;
					Vector2 cpoint = Vector2.Lerp(points[i], points[inext], 0.5f);
					//Check from Point A
					if (CivMath.LineSegmentIntersection(points[i] - off, points[i] + off, oPoints[o], oPoints[(o + 1) % 4], out s))
					{
						var dist = Mathf.Abs(points[i].x - s.x);
						if (!closestFound[i] || closestDistances[i] > dist)
						{
							closestFound[i] = true;
							closestPosition[i] = oPoints[o];
							closestDistances[i] = dist;
						}
					}
					//Check from Point B
					else if (CivMath.LineSegmentIntersection(points[inext] - off, points[inext] + off, oPoints[o], oPoints[(o + 1) % 4], out s))
					{
						var dist = Mathf.Abs(points[inext].x - s.x);
						if (!closestFound[i] || closestDistances[i] > dist)
						{
							closestFound[i] = true;
							closestPosition[i] = oPoints[o];
							closestDistances[i] = dist;
						}
					}
					//Check for center point (this is not a 100% hit solution)
					else if (CivMath.LineSegmentIntersection(cpoint - off, cpoint + off, oPoints[o], oPoints[(o + 1) % 4], out s))
					{
						var dist = Mathf.Abs(cpoint.x - s.x);
						if (!closestFound[i] || closestDistances[i] > dist)
						{
							closestFound[i] = true;
							closestPosition[i] = oPoints[o];
							closestDistances[i] = dist;
						}
					}
				}
			}

			//We apply the docking position, we choose the closest point of one axis (left vs right, top vs bottom)
			if (closestFound[0])
			{
				pos.x = closestPosition[0].x;
			}
			if (closestFound[2] && closestDistances[2] < closestDistances[0])
			{
				pos.x = closestPosition[2].x - d.rect.rect.size.x;
			}
			if (closestFound[1])
			{
				pos.y = closestPosition[1].y - d.rect.rect.size.y;
			}
			if (closestFound[3] && closestDistances[3] < closestDistances[1])
			{
				pos.y = closestPosition[3].y;
			}
		}

		private void UnloadDockableWindows(Scene scene, LoadSceneMode mode)
		{
			var elements = transform.GetComponentsInChildren<DockableElement>(true);
			for(int i = elements.Length - 1; i >= 0; i--)
			{
				var element = elements[i];
				if (element == null)
					continue;
				if (element.surviveSceneLoad)
					continue;				
				Destroy(element.gameObject);
			}
		}
	}
}
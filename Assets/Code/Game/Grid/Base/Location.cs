using UnityEngine;
using System;
using System.Linq;

namespace Endciv
{
	[Serializable]
	public class LocationSaveData : ISaveable
	{
		public int destinationType;
		public Vector2i[] indecies;
		public SerVector2[] positions;
		public string structureUID;
		public int currentPositionID;

		public ISaveable CollectData()
		{
			return this;
		}

		public Location ToLocation()
		{
			var loc = new Location();
			loc.Type = (Location.EDestinationType)destinationType;
			loc.Indecies = indecies;
			loc.Positions = positions.ToVector2Array();
			if (!string.IsNullOrEmpty(structureUID))
			{
                var guid = Guid.Parse(structureUID);
				if (Main.Instance.GameManager.SystemsManager.Entities.ContainsKey(guid))
					loc.Structure = Main.Instance.GameManager.SystemsManager.Entities[guid];
			}
			loc.currentPositionID = currentPositionID;
			return loc;
		}
	}

	public class Location : ISaveable
	{
		public enum EDestinationType { Position, Structure, Multiple, Waypoint }
		public EDestinationType Type;
		public Vector2i Index
		{
			get
			{
				if (Indecies == null || Indecies.Length <= 0) return Vector2i.Zero;
				return Indecies[0];
			}
		}
		public Vector2i[] Indecies;

		public Vector2[] Positions;
		public BaseEntity Structure;
		public int currentPositionID;
		public int uid;

		public Location()
		{
			uid = CivRandom.Range(0, int.MaxValue - 1);
		}
		public Location(Vector2i node)
		{
			uid = CivRandom.Range(0, int.MaxValue - 1);
			Type = EDestinationType.Position;
			Indecies = new Vector2i[] { node };
		}
		public Location(Vector2i[] nodes)
		{
			uid = CivRandom.Range(0, int.MaxValue - 1);
			Type = EDestinationType.Multiple;
			Indecies = nodes;
		}

		public Location(Vector2 node)
		{
			uid = CivRandom.Range(0, int.MaxValue - 1);
			Type = EDestinationType.Waypoint;
			Positions = new Vector2[] { node };
		}

		public Location(Vector2[] nodes)
		{
			uid = CivRandom.Range(0, int.MaxValue - 1);
			Type = EDestinationType.Waypoint;
			Positions = nodes;
		}
		public Location(BaseEntity structure, bool toEntrancePoints)
		{
			uid = CivRandom.Range(0, int.MaxValue - 1);
			Type = EDestinationType.Structure;
			Structure = structure;
			if (toEntrancePoints && structure.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints != null && structure.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints.Length > 0)
			{
				//set entrnace points as targets, shall we rather copy them?
				Indecies = structure.GetFeature<GridObjectFeature>().GridObjectData.EntrancePoints;
			}
			else
			{
				Indecies = structure.GetFeature<GridObjectFeature>().GridObjectData.Rect.ToArray();
			}
		}

		public Location GetCopy()
		{
			Location l = null;
			if (Type == EDestinationType.Position)
				l = new Location(Indecies[0]);
			else if (Type == EDestinationType.Waypoint)
				l = new Location(Positions);
			else if (Type == EDestinationType.Multiple)
				l = new Location(Indecies);
			else if (Type == EDestinationType.Structure)
			{
				l = new Location();
				l.Type = EDestinationType.Structure;
				l.Indecies = Indecies;
			}
			return l;
		}

		public ISaveable CollectData()
		{
			var data = new LocationSaveData();
			data.destinationType = (int)Type;
			data.indecies = Indecies;
			data.positions = Positions.ToSerVector2Array();
			if (Structure != null)
				data.structureUID = Structure.UID.ToString();
			else
				data.structureUID = string.Empty;
			data.currentPositionID = currentPositionID;
			return data;
		}

		public override string ToString()
		{
			string indecies = "null";
			if (Indecies != null)
				indecies = string.Join("|", Indecies.Select(x => x.ToString()).ToArray());
			string positions = "null";
			if (Positions != null)
				positions = string.Join("|", Positions.Select(x => x.ToString()).ToArray());
			return "Destination type : " + Type + "\n" +
			"UID : " + uid + "\n" +
			"Index : " + Index + "\n" +
			"Indecies : " + indecies + "\n" +
			"Positions : " + positions + "\n" +
			"Current Position ID : " + currentPositionID;
		}
	}
}
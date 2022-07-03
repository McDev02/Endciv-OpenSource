using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class UI3DFactory : MonoBehaviour
	{
		public static UI3DFactory Instance { get; private set; }
		private static UI3DFactory m_Instance;

		private List<UI3DBase> m_Pool = new List<UI3DBase>();

		[SerializeField]
		private UI3DBase[] m_Prefabs = ArrayUtil<UI3DBase>.Empty;
		[SerializeField] UI3DController controller;

		public const string IconHunger = "hunger";
		public const string IconThirst = "thirst";
		public const string IconCattle = "cattle";
		public const string IconHomeless = "homeless";
		public const string IconImmigrant = "immigrant";
		public const string IconTrader = "trader";
		public const string IconDeath = "death";

		public const string IconExpedition = "expedition";

		public enum ESignIcon { Expedition }

		public enum EIcon
		{
			//DO not change order, add new items at the end
			Collect, Alarm, Construction, Demolish, Repair, Chop
		}

		public Sprite m_CollectIcon;
		public Sprite m_AlarmIcon;
		public Sprite m_ConstructionIcon;
		public Sprite m_DemolishIcon;
		public Sprite m_ChopIcon;
		public Sprite m_RepairIcon;
		public Sprite m_PeopleAdd;
		public Sprite m_PeopleMinus;
		public Sprite m_PeopleDie;

		[SerializeField] Image needIconPrefab;
		Stack<Image> needIconPool;

		private void Awake()
		{
			Instance = this;
		}

		//Specifics
		public UI3DConstruction GetUI3DConstruction(ConstructionFeature owner, bool demolition, float heightOffset = 2)
		{
			var pop = Create<UI3DConstruction>();
			pop.Setup(owner, heightOffset, demolition);
			return pop;
		}

		public UI3DNeeds GetUI3DNeeds(BaseEntity owner, float heightOffset = 2)
		{
			var pop = Create<UI3DNeeds>();
			pop.Setup(owner, heightOffset);
			return pop;
		}

		public UI3DSign GetUI3DSign(Vector3 worldPosition, ESignIcon icon)  //Todo: Add icon selection once we need multiple different signs
		{
			var sign = Create<UI3DSign>();
			Sprite sprite = null;
			switch (icon)
			{
				case ESignIcon.Expedition:
					sprite = ResourceManager.Instance.GetIcon(IconExpedition, EResourceIconType.Notification);
					break;
			}
			sign.Setup(worldPosition, sprite);
			return sign;
		}

		//General
		public UI3DResourcePop GetUI3DResourcePop(BaseEntity owner, Vector3 position, ResourceStack item, bool woble = false, bool scaleUp = false)
		{
			var pop = Create<UI3DResourcePop>();
			pop.Setup(owner, position, item, woble, scaleUp);
			return pop;
		}

		public UI3DResourcePop GetUI3DResourcePop(BaseEntity owner, Vector3 position, Sprite icon, bool woble = false, bool scaleUp = false)
		{
			var pop = Create<UI3DResourcePop>();
			pop.Setup(owner, position, icon, woble, scaleUp);
			return pop;
		}

		public UI3DTextPop GetUI3DTextPop(BaseEntity owner, Vector3 position, string text, Color textColor, bool woble = false, bool scaleUp = false)
		{
			var pop = Create<UI3DTextPop>();
			pop.Setup(owner, position, text, textColor, woble, scaleUp);
			return pop;
		}

		public UI3DResourcePulse GetUI3DResourcePulse(BaseEntity owner, Vector3 position, ResourceStack item)
		{
			//Load Sprite from item data
			Sprite sprite = null;
			return GetUI3DResourcePulse(owner, position, sprite);
		}

		public UI3DResourcePulse GetUI3DResourcePulse(BaseEntity owner, Vector3 position, Sprite icon)
		{
			var pulse = Create<UI3DResourcePulse>();
			pulse.Setup(owner, position, icon);
			return pulse;
		}
		public UI3DIconMark ShowIconMark(Transform trans, EIcon iconType, float heightOffset = 0)
		{
			var link = Instance.Create<UI3DIconMark>();
			link.Setup(trans, heightOffset);
			switch (iconType)
			{
				case EIcon.Collect:
					link.m_Icon.sprite = Instance.m_CollectIcon;
					break;
				case EIcon.Alarm:
					link.m_Icon.sprite = Instance.m_AlarmIcon;
					break;
				case EIcon.Construction:
					link.m_Icon.sprite = Instance.m_ConstructionIcon;
					break;
				case EIcon.Demolish:
					link.m_Icon.sprite = Instance.m_DemolishIcon;
					break;
				case EIcon.Chop:
					link.m_Icon.sprite = Instance.m_ChopIcon;
					break;
				case EIcon.Repair:
					link.m_Icon.sprite = Instance.m_RepairIcon;
					break;
				default:
					break;
			}
			return link;
		}

		public T Create<T>()
			where T : UI3DBase
		{
			for (int i = m_Pool.Count - 1; i >= 0; i--)
			{
				if (m_Pool[i] == null)
				{
					m_Pool.RemoveAt(i);
					continue;
				}
				if (m_Pool[i] is T)
				{
					var ui = (T)m_Pool[i];
					m_Pool.RemoveAt(i);
					ui.gameObject.SetActive(true);
					controller.AddElement(ui);
					return ui;
				}
			}
			for (int i = 0; i < m_Prefabs.Length; i++)
			{
				if (m_Prefabs[i] is T)
				{
					var ui = CivHelper.Instantiate<T>((T)m_Prefabs[i]);
					ui.transform.SetParent(transform, false);
					controller.AddElement(ui);
					return ui;
				}
			}
			return null;
		}

		public void Recycle(UI3DBase ui)
		{
			if (ui != null)
			{
				ui.gameObject.SetActive(false);
				controller.RemoveElement(ui);
				m_Pool.Add(ui);
			}
		}
	}
}
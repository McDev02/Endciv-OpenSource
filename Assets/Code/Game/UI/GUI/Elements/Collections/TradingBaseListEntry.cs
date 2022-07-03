using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public abstract class TradingBaseListEntry : MonoBehaviour
	{
		public Image icon;
		public Text amount;
		public Text value;
		public EIconSize iconSize;
		[SerializeField] protected UITooltipText tooltip;

		protected TradingWindow tradingWindow;
		public string id;
		public int currentAmount;

		public object[] args;

		public abstract float BaseValue { get; }
		public abstract bool Matches(TradingBaseListEntry entry);

		public abstract void DestroyResources();
		public abstract void AcquireResources();

		public virtual void Setup(int amount, string staticDataID, TradingWindow tradingWindow, bool useTooltip = true, params object[] args)
		{
			this.amount.text = amount.ToString();
			currentAmount = amount;
			id = staticDataID;						
			value.text = BaseValue.ToString();
			this.tradingWindow = tradingWindow;

			if (useTooltip)
				SetupTooltip();
			this.args = args;
		}

		public abstract void SetupTooltip();

		internal void UpdateValues()
		{
			amount.text = currentAmount.ToString();
		}

		public void Add(int amount)
		{
			var transferedAmount = Mathf.Min(currentAmount, amount);
			tradingWindow.MoveResources(transferedAmount, this);
		}
		public void AddAll()
		{
			tradingWindow.MoveResources(currentAmount, this);
		}
	}
}
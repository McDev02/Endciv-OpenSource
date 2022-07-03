using UnityEngine.UI;

namespace Endciv
{
	public class ObjectiveWindow : GUIAnimatedPanel
	{
		public Button nextButton;
		public Button previousButton;
		public Button OKButton;

		private string[] pages;
		private int index;

		public Text title;
		public TextMeshParser textParser;

		public void Setup(string[] pages, string titleText)
		{
			this.pages = pages;
			title.text = titleText;
			if (pages.Length <= 1)
			{
				OKButton.gameObject.SetActive(true);
				nextButton.gameObject.SetActive(false);
				previousButton.gameObject.SetActive(false);
			}
			else
			{
				OKButton.gameObject.SetActive(false);
				nextButton.gameObject.SetActive(true);
				previousButton.gameObject.SetActive(false);
			}
			index = 0;
			if (pages.Length > 0)
			{
				textParser.locaID = pages[0];
			}
		}

		public void NextPage()
		{
			if (index >= pages.Length - 1)
				return;
			index++;
			textParser.locaID = pages[index];

			var next = index == pages.Length - 1;
			OKButton.gameObject.SetActive(next);
			nextButton.gameObject.SetActive(!next);
			previousButton.gameObject.SetActive(true);
		}

		public void PreviousPage()
		{
			if (index <= 0)
				return;
			index--;
			textParser.locaID = pages[index];

			OKButton.gameObject.SetActive(false);
			nextButton.gameObject.SetActive(true);
			previousButton.gameObject.SetActive(index > 0);
		}

		public void Complete()
		{
			OnClose();
		}
	}
}
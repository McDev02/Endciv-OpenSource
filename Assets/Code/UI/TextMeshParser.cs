using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;

namespace Endciv
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class TextMeshParser : MonoBehaviour
	{
		private const string headerPattern = @"<h.*?>(.*?)</h.*?>";
		private const string headerContentPattern = @"<.*>(.*?)</.*>";
		private const string imagePattern = @"<img.*?\/>";

        [LocaId]
        public string locaID;
        private TextMeshProUGUI tmPro;
		private string previousLocaID = string.Empty;


#if UNITY_EDITOR
		private void OnValidate()
		{
			if (tmPro == null)
				Awake();
            previousLocaID = locaID;
			OnTextChanged();
		}
#endif

		private void Awake()
		{
			tmPro = GetComponent<TextMeshProUGUI>();
		}

        private void Start()
        {
            OnTextChanged();
        }

        private void Update()
		{
			if (previousLocaID != locaID)
			{
                previousLocaID = locaID;
				OnTextChanged();
			}
		}

		private void OnTextChanged()
		{
            string inputText = string.Empty;
            LocalizationManager.GetTextSafely(locaID, out inputText, LocalizationManager.ETextStyle.Normal);
            var resourceManager = ResourceManager.Instance;
			string textToSend = inputText;
			//Headers
			MatchCollection headerMatches = Regex.Matches(textToSend, headerPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (Match match in headerMatches)
			{
				MatchCollection contentMatches = Regex.Matches(match.Groups[1].Value, headerContentPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
				if (contentMatches.Count > 0)
				{
					foreach (Match contentMatch in contentMatches)
					{
						string upperCase = contentMatch.Groups[1].Value.ToUpper();
						textToSend = textToSend.Replace(contentMatch.Groups[1].Value, upperCase);
					}
				}
				else if (!string.IsNullOrEmpty(match.Groups[1].Value))
				{
					string upperCase = match.Groups[1].Value.ToUpper();
					textToSend = textToSend.Replace(match.Groups[1].Value, upperCase);
				}
			}
			textToSend = textToSend.Replace("<h1>", "<size=+10>");
			textToSend = textToSend.Replace("</h1>", "</size>");
			textToSend = textToSend.Replace("<h2>", "<size=+5>");
			textToSend = textToSend.Replace("</h2>", "</size>");
			textToSend = textToSend.Replace("<h3>", "<b>");
			textToSend = textToSend.Replace("</h3>", "</b>");

			//Paragraphs
			textToSend = textToSend.Replace("<p>", "\n<size=5>\n</size>");
			textToSend = textToSend.Replace("</p>", "\n");

			//Images     
			MatchCollection imageMatches = Regex.Matches(textToSend, imagePattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
			foreach (Match match in imageMatches)
			{
				string imgName = string.Empty;
				int startIndex = match.Value.IndexOf('[') + 1;
				if (startIndex >= 1)
				{
					imgName = match.Value.Substring(startIndex, match.Value.LastIndexOf(']') - startIndex);
					string imageEntry = "<sprite name=\"" + imgName + "\">";

					bool absOrRel = true;
					startIndex = match.Value.ToLower().IndexOf("scale=");
					if (startIndex >= 0)
					{
						string val = match.Value.Substring(startIndex);
						absOrRel = !val.ToLower().Contains("rel");						
					}

					if (absOrRel)
					{
						if (resourceManager != null)
						{
							Vector2 size = resourceManager.GetSpriteSize(imgName);
							imageEntry = "<size=" + (int)size.y + ">" + imageEntry + "</size>";
						}
					}

					textToSend = textToSend.Replace(match.Value, imageEntry);
				}
				else
				{
					textToSend = textToSend.Replace(match.Value, "");
				}
			}
			tmPro.text = textToSend;
		}
	}
}


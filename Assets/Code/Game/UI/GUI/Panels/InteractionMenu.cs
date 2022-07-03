using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace Endciv
{
	public class InteractionMenu : GUIAnimatedPanel
	{
		[SerializeField] ConstructionMenu constructionMenu;

		public void Setup(GameManager gameManager)
		{
			constructionMenu.Setup(gameManager);
		}

		public void Run()
		{
			constructionMenu.Run();
		}
	}
}
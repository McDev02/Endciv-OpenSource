using UnityEngine;
namespace Endciv
{
	public class FactoryManager : MonoBehaviour
	{
		private GameManager GameManager;

		public NotificationFactory NotificationFactory;

        public ModelFactory ModelFactory;

		public SimpleEntityFactory SimpleEntityFactory;

        public void Setup(GameManager gameManager)
		{
			GameManager = gameManager;
						
			NotificationFactory = new NotificationFactory();			
            if (ModelFactory == null)
                ModelFactory = new ModelFactory("SimpleModels/");
			if(SimpleEntityFactory == null)
			{
				SimpleEntityFactory = new SimpleEntityFactory();
				SimpleEntityFactory.Setup("StaticData/SimpleEntities", gameManager.SystemsManager);
			}
		}
	}
}
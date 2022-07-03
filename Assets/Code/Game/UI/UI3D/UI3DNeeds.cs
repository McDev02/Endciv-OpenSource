using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	//Todo: Improve pooling of icons
	public class UI3DNeeds : UI3DBase
	{
        [SerializeField] Image imagePrefab;
		const int maxDistance = 80;
		const int minDistance = 50;
		public Dictionary<string, Image> images;
        private Stack<Image> imagePool;

		public void Setup(BaseEntity baseEntity, float heightOffset)
		{
			base.Setup(baseEntity.GetFeature<EntityFeature>().View.transform, heightOffset);
            if (imagePool == null)
                imagePool = new Stack<Image>();
            if(images == null)
			    images = new Dictionary<string, Image>();
            else
            {
                foreach(var pair in images)
                {
                    if (pair.Value == null)
                        continue;
                    pair.Value.gameObject.SetActive(false);
                    imagePool.Push(pair.Value);
                }
                images.Clear();
            }
		}

		public override void UpdateElement(Vector3 camPos)
		{
			float dist = (transform.position - camPos).magnitude;
			var show = (dist <= maxDistance);

            foreach(var pair in images)
            {
                pair.Value.enabled = show;
            }			
		}

		protected override void Dispose()
		{
			UI3DFactory.Instance.Recycle(this);
		}

        public void AddImage(string iconID)
        {
            if (images.ContainsKey(iconID))
                return;
            Image img = null;
            while(img == null && imagePool.Count > 0)
            {
                img = imagePool.Pop();
            }
            if(img == null)
            {
                img = Instantiate(imagePrefab, transform);
            }
            img.gameObject.SetActive(true);
            img.overrideSprite = ResourceManager.Instance.GetIcon(iconID, EResourceIconType.Notification);
            images.Add(iconID, img);
        }

        public void RemoveImage(string iconID)
        {
            if (!images.ContainsKey(iconID))
                return;
            images[iconID].gameObject.SetActive(false);
            imagePool.Push(images[iconID]);
            images.Remove(iconID);
        }
	}
}
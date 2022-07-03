using UnityEngine;
using UnityEngine.UI;

namespace Endciv
{
	public class GUIImmigrantGroupListEntry : MonoBehaviour
	{
		public Text peopleInfo;
		public Text remainingTime;

        private ImmigrationGroup group;

        public void Setup(ImmigrationGroup group)
        {
            int adultCount = 0;
            int childCount = 0;
            foreach(var immigrant in group.immigrants)
            {
				var age = immigrant.Entity.GetFeature<LivingBeingFeature>().age;
                if (age == ELivingBeingAge.Child)
                    childCount++;
                else
                    adultCount++;
            }
            string info = string.Empty;
            if(adultCount > 0)
            {
                if(adultCount == 1)
                {
                    info += "1 Adult";
                }
                else
                {
                    info += adultCount + " Adults";
                }
                info += "\n";
            }
            if (childCount > 0)
            {
                if (childCount == 1)
                {
                    info += "1 Child";
                }
                else
                {
                    info += childCount + " Children";
                }
                info += "\n";
            }
            peopleInfo.text = info;

            var timespan = System.TimeSpan.FromMinutes(group.timeRemaining);
            remainingTime.text = string.Format("{0:00}:{1:00}", timespan.Hours, timespan.Minutes);
            this.group = group;
        }

        public void AcceptCall()
        {
            Main.Instance.GameManager.SystemsManager.NpcSpawnSystem.ConvertImmigrantsToCitizens(group);
        }

        public void DenyCall()
        {
            Main.Instance.GameManager.SystemsManager.NpcSpawnSystem.DenyImmigrationGroup(group);
        }
	}
}
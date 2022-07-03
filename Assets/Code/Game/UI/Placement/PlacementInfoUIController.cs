using System.Collections.Generic;
using UnityEngine;

namespace Endciv
{
    public class PlacementInfoUIController : MonoBehaviour
    {
        public UserInfo userInfoPrefab;

        private MiningSystem miningSystem;
        private List<UserInfo> userInfoList = new List<UserInfo>();
        private Stack<UserInfo> userInfoPool = new Stack<UserInfo>();

        private EntityStaticData currentEntity;
        private GridObjectData gridObjectData;        
        private float radius = 0f;

        public void Setup()
        {
            miningSystem = Main.Instance.GameManager.SystemsManager.MiningSystem;
            userInfoPrefab.Run(Main.Instance.GameManager.CameraController, Main.Instance.gameInputManager);
        }                  

        public void Run(EntityStaticData entity, GridObjectData gridObjectData)
        {            
            if(currentEntity != null)
            {
                Stop();
            }
            currentEntity = entity;
            this.gridObjectData = gridObjectData;
            radius = 2f;
            if (currentEntity.HasFeature(typeof(MiningStaticData)))
            {
                var miningData = currentEntity.GetFeature<MiningStaticData>();
                if (miningData.miningType == EMiningType.Groundwater)
                {
                    radius += miningData.radius;
                }
            }
        }

        void Update()
        {
            if (currentEntity == null)
                return;
            CleanupControls();
            var wells = miningSystem.FeaturesByFaction[SystemsManager.MainPlayerFaction];
            foreach(var well in wells)
            {
                if (well.StaticData.miningType != EMiningType.Groundwater)
                    continue;
                var totalRadius = radius + well.StaticData.radius;
                var wellGridObjectData = well.Entity.GetFeature<GridObjectFeature>().GridObjectData;
                var rect = wellGridObjectData.Rect;
                if (Vector2.Distance(gridObjectData.Rect.Center, rect.Center) > totalRadius)
                    continue;

				var wellData = miningSystem.CalculateGain(well);//.StaticData, rect);
				var text = $"{LocalizationManager.GetText("#UI/Game/UserTool/Efficiency")}: {(int)(100 * wellData.efficientcy)}%";
				
                var info = GetUserInfo();
                info.SetText(text);
                var pos = well.Entity.GetFeature<EntityFeature>().View.transform.position;
                info.SetPosition(pos, 2);
            }
        }

        public void Stop()
        {
            CleanupControls();
            currentEntity = null;
            gridObjectData = null;
        }

        private void CleanupControls()
        {
            for (int i = userInfoList.Count - 1; i >= 0; i--)
            {
                var userInfo = userInfoList[i];
                RemoveUserInfo(userInfo);
            }
        }

        private UserInfo GetUserInfo()
        {
            UserInfo userInfo = null;
            while (userInfo == null && userInfoPool.Count > 0)
            {
                userInfo = userInfoPool.Pop();
            }
            if (userInfo == null)
                userInfo = Instantiate(userInfoPrefab, transform);
            userInfo.gameObject.SetActive(true);
            userInfoList.Add(userInfo);
            return userInfo;
        }

        private void RemoveUserInfo(UserInfo userInfo)
        {
            userInfoList.Remove(userInfo);
            userInfo.gameObject.SetActive(false);
            userInfoPool.Push(userInfo);
        }
    }
}
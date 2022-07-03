using UnityEngine;

namespace Endciv
{
	public class EntityFeature :
		Feature<EntityFeatureSaveData>,
		IViewController<EntityFeatureView>
	{
		public EntityFeatureStaticData StaticData { get; private set; }
		/// <summary>
		/// Entity's position in grid coordinates
		/// </summary>
		public Vector2i GridID;

		/// <summary>
		/// Entity's partition coordinates
		/// </summary>
		public Vector2i PartitionID;

		//Properties	
		public string EntityName;        //Custom name of the object	
		public bool Invincible { get; private set; }
		public bool IsAlive;
		public EntityProperty Health { get; private set; } //Current health. Value of 0 means death	
		public EntityProperty Stress { get; private set; } //Stamina or Stress which cause exhaustion/overusage and failure for living beings and machines
		public float currentStressLevel;
		public int BornTimeTick { get; private set; }

		//float Combustion;	//How much it is burned down. 0 means death	
		//float Wettness;	//How wet an object is
		//int Inflamability;  // Likelines to burn

		public EntityFeatureView View { get; private set; }

		public override void Setup(BaseEntity entity, FeatureParamsBase args = null)
		{
			base.Setup(entity);
			if(args != null)
			{
				var featureParams = (EntityFeatureParams)args;
				if (featureParams != null)
				{
					Entity.ChangeFaction(featureParams.FactionID);
				}
			}		
			StaticData = Entity.StaticData.GetFeature<EntityFeatureStaticData>();
			IsAlive = true;
			EntityName = entity.StaticData.Name;
			Invincible = StaticData.Invincible;
			Health = new EntityProperty(Invincible ? 999 : StaticData.MaxHealth, Invincible ? 999 : StaticData.MaxHealth);
			Stress = new EntityProperty(1, 0);
		}

		public override void Run(SystemsManager manager)
		{
			base.Run(manager);
			BornTimeTick = manager.timeManager.CurrentTotalTick;
		}

		public override void Destroy()
		{
			base.Destroy();
			if (View != null)
			{
				Object.Destroy(View.gameObject);
				View = null;
			}
		}

		#region IViewController
		//Entity feature view is just an empty game object
		//Do not propagate any calls to the View

		public int CurrentViewID { get; set; }

		public void SetView(FeatureViewBase view)
		{
			View = (EntityFeatureView)view;
		}

		public void ShowView()
		{

		}

		public void HideView()
		{

		}

		public void UpdateView()
		{

		}

		public void SelectView()
		{

		}

		public void DeselectView()
		{

		}
		#endregion

		public override void ApplyData(EntityFeatureSaveData data)
		{
			GridID = data.gridPosition.ToVector2i();
			View.transform.position = data.position.ToVector3();
			View.transform.rotation = data.rotation.ToQuaternion();
			//Properties
			Health.Value = data.health;
			BornTimeTick = data.bornTimeTick;
			if (Health.Value <= 0)
				Entity.Die();
		}

		public override ISaveable CollectData()
		{
			var data = new EntityFeatureSaveData();
			//Base data
			data.position = SerVector3.FromVector3(View.transform.position);
			data.rotation = SerVector4.FromQuaternion(View.transform.rotation);
			data.gridPosition = new SerVector2i(GridID);

			//Properties
			data.health = Health.Value;
			data.bornTimeTick = BornTimeTick;
			return data;
		}
	}
}


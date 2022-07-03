using System;
using System.Collections.Generic;

namespace Endciv
{
	public sealed class FactoryParams
	{
		private Dictionary<Type, FeatureParamsBase> featureParams;

		public FactoryParams()
		{
			featureParams = new Dictionary<Type, FeatureParamsBase>();
		}

		public FactoryParams(params FeatureParamsBase[] args) 
			: base()
		{
			if (args != null && args.Length > 0)
			{
				foreach(var arg in args)
				{
					SetParams(arg);
				}
			}
		}

		public void SetParams(params FeatureParamsBase[] featureParams)
		{
			foreach(var param in featureParams)
			{
				SetParams(param);
			}
		}

		private void SetParams(FeatureParamsBase featureParam)
		{
			var paramType = featureParam.GetType();
			var featureType = paramType.BaseType.GetGenericArguments()[0];
			if (!HasParams(featureType))
				featureParams.Add(featureType, null);
			featureParams[featureType] = featureParam;
		}

		public FeatureParamsBase GetParams(Type featureType)
		{
			return featureParams[featureType];
		}

		public bool HasParams(Type featureType)
		{
			return featureParams.ContainsKey(featureType);
		}
	}
}

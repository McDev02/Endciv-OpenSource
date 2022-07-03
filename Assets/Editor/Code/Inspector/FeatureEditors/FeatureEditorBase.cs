namespace Endciv.Editor
{
    public abstract class FeatureEditorBase
    {
        public abstract void OnGUI();
        public abstract void SetFeature(FeatureBase feature);
        public virtual void OnEnable()
        {

        }
    }

    public abstract class FeatureEditor<T> : FeatureEditorBase
        where T : FeatureBase
    {
        public T Feature { get; private set; }

        public override void SetFeature(FeatureBase feature)
        {
            Feature = (T)feature;
        }
    }

}

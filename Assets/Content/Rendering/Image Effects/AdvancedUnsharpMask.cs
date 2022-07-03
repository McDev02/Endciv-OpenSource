/* 2012 by Kevin Scheitler - EyecyStudio
 * contact@eyecystudio.de
 */

using UnityEngine;

namespace Endciv
{
	[ExecuteInEditMode]
    [RequireComponent(typeof (Camera))]
    [AddComponentMenu("Image Effects/Advanced UnsharpMask")]
    public class AdvancedUnsharpMask : MonoBehaviour
    {
        private Material m_Material;

        public float Pixel = 0.5f;
        public float Amount = 1.0f;
        public float SecondAmount = 1.0f;
        public float Threshold = 0.1f;
        public float m_DepthBias = 1.0f;
        public float m_Power = 1.0f;

        public bool m_UseDepth;
        public Shader m_NormalShader;
        public Shader m_DepthShader;

        private Shader Shader
        {
            get { return m_UseDepth ? m_DepthShader : m_NormalShader; }
        }

        private Material Material
        {
            get
            {
                if (m_Material == null || m_Material.shader != Shader)
                {
                    m_Material = new Material(Shader);
                    m_Material.hideFlags = HideFlags.HideAndDontSave;
                }
                return m_Material;
            }
        }

        private void Start()
        {
            if (Shader == null)
            {
                Debug.Log("Shaders are not set up!");
                enabled = false;
            }
            else
            {
                if (!Shader.isSupported)
                {
                    Debug.Log("Shader is not supported!");
                    enabled = false;
                }
            }
        }

        private void OnEnable()
        {
            GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
        }

        // Called by the camera to apply the image effect
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Material mat = Material;

            mat.SetFloat("_Pixel", Pixel);
            mat.SetFloat("_Amount", Amount);
            mat.SetFloat("_Amount2", SecondAmount);
            mat.SetFloat("_Threshold", Threshold);
            mat.SetFloat("_DepthBias", m_DepthBias);
            mat.SetFloat("_Fac", m_Power);

            mat.SetVector("_ScreenSize", new Vector4(Screen.width, Screen.height, 0, 0));

            Graphics.Blit(source, destination, mat);
        }

        internal void AddAmount()
        {
            Amount = (Amount + 0.5f)%3.5f;
            if (Amount == 0)
                this.enabled = false;
            else this.enabled = true;
        }
    }
}

// 2012 by Kevin Scheitler - EyecyStudio
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
namespace Endciv
{
	public class ToolMain : MonoBehaviour
	{
		public enum EScene { Main, Menu, StaticDataEditor }
		public EScene CurrentScene { get; private set; }

		public GameManager GameManager { get; private set; }

		private void Awake()
		{
			DontDestroyOnLoad(this);
		}

		private void Start()
		{
			SwitchScene(EScene.Menu);
		}

		public void SwitchScene(EScene scene)
		{
			if (CurrentScene == scene) return;
			StartCoroutine(LoadScene(scene));
		}

		private IEnumerator LoadScene(EScene scene)
		{
			yield return SceneManager.LoadSceneAsync((int)scene);
			CurrentScene = scene;
		}
	}
}
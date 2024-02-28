using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace MinimalistTLBinder.Example {
	public class MinimalistBinderExample : MonoBehaviour {

		[SerializeField]
		private GameObject rootNode = default;
		[SerializeField]
		private List<GameObject> timelinePrefabs = default;

		private GameObject loadedTimeline = default;
		private Dictionary<string, BindableAsset> bindableReferences = new ();

		private void Start()
		{
			CacheBindableAssets();
		}

		private void CacheBindableAssets() {
			bindableReferences.Clear();

			var bindableAssets = rootNode.GetComponentsInChildren<BindableAsset>(true);
			foreach (var bindableAsset in bindableAssets) {
				bindableReferences[bindableAsset.Id] = bindableAsset;
			}
		}

		private void LoadDirector(int index) {
			CleanupTimeline();
			
			loadedTimeline = Instantiate(timelinePrefabs[index]);
			var binder = loadedTimeline.GetComponent<PlayableDirectorBinder>();
			var requiredIds = binder.GetRequiredBindableIds();


			foreach (var id in requiredIds) {
				binder.Bind(bindableReferences[id]);
			}
		}

		private void CleanupTimeline() {
#if UNITY_EDITOR
			DestroyImmediate(loadedTimeline);
#else
			Destroy(loadedTimeline);
#endif
		}

		private void Update() {
			if (Input.GetKeyUp(KeyCode.Alpha1)) {
				LoadDirector(0);
			}
			else if (Input.GetKeyUp(KeyCode.Alpha2)) {
				LoadDirector(1);
			}
			else if (Input.GetKeyUp(KeyCode.Alpha3)) {
				LoadDirector(2);
			}
			else if (Input.GetKeyUp(KeyCode.C)) {
				CleanupTimeline();
			}
			else if (Input.GetKeyUp(KeyCode.P)) {
				PlayLoadedTimeline();
			}

		}
		private void PlayLoadedTimeline() {
			if (loadedTimeline == null) {
				Debug.LogWarning("Need to load a timeline first...");
				return;
			}
			var director = loadedTimeline.GetComponent<PlayableDirector>();
			if (director == null) {
				Debug.LogWarning("PlayableDirector wasn't found...");
				return;
			}

			director.Stop();
			director.Play();
		}

	}
}

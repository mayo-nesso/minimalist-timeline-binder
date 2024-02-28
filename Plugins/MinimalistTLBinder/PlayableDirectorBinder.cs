using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace MinimalistTLBinder {
	public class PlayableDirectorBinder : MonoBehaviour {
		[SerializeField]
		private PlayableDirector director = default;

		[SerializeField]
		private bool tryBindOnAwake = true;

		public PlayableDirector Director => director;

		private Dictionary<string, List<PlayableBinding>> bindableReferences = null;

		private void Awake() {
			if (tryBindOnAwake) {
				SearchAndBindObjectsOnScene();
			}
		}

		[ContextMenu("Wire binding references")]
		private void WireBindingReferences() {
			foreach (var playableBinding in director.playableAsset.outputs) {

				var targetObject = director.GetGenericBinding(playableBinding.sourceObject);
				if (targetObject == null) {
					continue;
				}

				var sourceGameObject = GetGameObjectFromTarget(targetObject);
				if (sourceGameObject == null) {
					continue;
				}

				var bindableAsset = GetBindableAssetFrom(sourceGameObject);
				var sourceBindableId = bindableAsset.Id;

				var trackAssetName = playableBinding.streamName;
				var trackDefinedBindableId = MTLBHelper.ExtractBindableId(trackAssetName);

				if (trackDefinedBindableId != sourceBindableId) {
					Debug.LogWarning($"playableBinding.streamName, ie; trackAssetName: [{trackAssetName}] " +
						$"doesn't specify required id:[{sourceBindableId}]. Fixing it...");

					var trackName = playableBinding.sourceObject.GetType().Name;
					var targetType = playableBinding.outputTargetType.ToString();
					playableBinding.sourceObject.name = MTLBHelper.FormatTrackAssetName(trackName, targetType, sourceBindableId);
				}

				bindableAsset.SetBindingObject(targetObject);
			}
		}

		[ContextMenu("Search and Bind objects on scene")]
		public void SearchAndBindObjectsOnScene() {
			UpdateBindableReferences();

			var allBindableAssets = MTLBHelper.GetBindableAssetsOnScene();

			foreach (var bindable in allBindableAssets) {
				TryToBind(bindable);
			}
		}

		[ContextMenu("Cleanup bindings")]
		public void CleanupBindings() {
			foreach (var playableBinding in director.playableAsset.outputs) {
				director.SetGenericBinding(playableBinding.sourceObject, null);
			}
		}

		[ContextMenu("Test Cleanup Bind scene elements")]
		public void TestCleanupLoad() {
			CleanupBindings();
			SearchAndBindObjectsOnScene();
		}


		[ContextMenu("Wire elements and Test Cleanup Bind")]
		public void WireAndTest() {
			WireBindingReferences();
			CleanupBindings();
			SearchAndBindObjectsOnScene();
		}

		public void ResetAllTrackNames() {
			ResetTrackNames(true);
		}

		public void ResetOnlyAssignedTrackNames() {
			ResetTrackNames(false);
		}

		private void ResetTrackNames(bool resetUnassignedTracks = false) {
			foreach (var playableBinding in director.playableAsset.outputs) {

				var skipNameReset = director.GetGenericBinding(playableBinding.sourceObject) == null;
				if (skipNameReset && !resetUnassignedTracks) {
					continue;
				}

				var trackName = playableBinding.sourceObject.GetType().Name;
				playableBinding.sourceObject.name = trackName;
			}
		}

		private GameObject GetGameObjectFromTarget(Object target) {
			switch (target) {
				case Component component:
					return component.gameObject;
				case GameObject gObject:
					return gObject;
			}

			Debug.LogWarning($"Can't find GameObject from target [{target}] :/ ");
			return null;
		}

		private BindableAsset GetBindableAssetFrom(GameObject targetGameObject) {
			var bindableAsset = targetGameObject.GetComponent<BindableAsset>();
			if (bindableAsset == null) {
				Debug.LogWarning("GameObject doesn't include a BindableAsset. Fixing it...");
				bindableAsset = targetGameObject.AddComponent<BindableAsset>();
			}

			return bindableAsset;
		}

		// Goes through all the playableBindings looking for binding Ids
		// when found one, add the reference to bindableReferences dictionary
		private void UpdateBindableReferences() {

			if (bindableReferences == null) {
				bindableReferences = new Dictionary<string, List<PlayableBinding>>();
			}
			bindableReferences.Clear();

			foreach (var playableBinding in director.playableAsset.outputs) {
				var bindingName = playableBinding.streamName;

				var bindingId = MTLBHelper.ExtractBindableId(bindingName);
				if (string.IsNullOrEmpty(bindingId)) {
					continue;
				}

				if (!bindableReferences.ContainsKey(bindingId)) {
					bindableReferences[bindingId] = new List<PlayableBinding>();
				}

				bindableReferences[bindingId].Add(playableBinding);
			}
		}

		public List<string> GetRequiredBindableIds() {
			if (bindableReferences == null) {
				UpdateBindableReferences();
			}

			return bindableReferences.Keys.ToList();
		}

		private void TryToBind(BindableAsset bindableAsset) {
			Bind(bindableAsset, true, false);
		}

		public void Bind(BindableAsset bindableAsset, bool muteErrors = false, bool force = true) {
			var bindingId = bindableAsset.Id;

			if (bindableReferences == null) {
				UpdateBindableReferences();
			}

			if (!bindableReferences.ContainsKey(bindingId)) {
				if (!muteErrors) {
					Debug.LogError($"Cannot find [{bindingId}] on cached references. Aborting...");
				}
				return;
			}

			var playableBindings = bindableReferences[bindingId];

			foreach (var playableBinding in playableBindings) {
				if (director.GetGenericBinding(playableBinding.sourceObject) == null || force) {
					director.SetGenericBinding(playableBinding.sourceObject, bindableAsset.GetBindingObject());
				}
			}
		}

	}
}

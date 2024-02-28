using System.Linq;
using UnityEngine;

namespace MinimalistTLBinder {
	public class BindableAsset : MonoBehaviour {
		[SerializeField]
		private string id = default;

		[SerializeField]
		private Object objectToBind = default;

		public string Id => id;

		private void OnValidate() {
			if (string.IsNullOrEmpty(id)) {
				Debug.LogWarning($"BindableAsset [{name}] hasn't an id assigned... Fixing it...");
				AssignBindId();
			}

			LookForAndFixDuplicatedIds();
		}

		private void AssignBindId(bool force = false) {
			if (!string.IsNullOrEmpty(id) && !force) {
				return;
			}
			id = string.Format($"{gameObject.name}_{GetBindingObject().GetHashCode()}");
		}

		private void LookForAndFixDuplicatedIds() {
			var allBindableAssets = MTLBHelper.GetBindableAssetsOnScene();

			var idCount = allBindableAssets.Count(x => x.Id == Id);
			var isThisIdUnique = idCount <= 1;
			if (isThisIdUnique) {
				return;
			}

			Debug.LogWarning($"There are [{idCount}] bindableAssets that have the same Id as bindableAsset [{name}]... Re assigning a new id...");
			AssignBindId(true);
		}

		public void SetBindingObject(UnityEngine.Object componentObject) {
			objectToBind = componentObject;
		}

		public UnityEngine.Object GetBindingObject() {
			return objectToBind != null ? objectToBind : gameObject;
		}

	}
}

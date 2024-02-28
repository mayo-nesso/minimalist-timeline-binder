using System.Collections;
using UnityEditor;
using UnityEngine;

namespace MinimalistTLBinder.Editor {

	[CustomEditor(typeof(PlayableDirectorBinder))]
	public class PlayableDirectorBinderEditor : UnityEditor.Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			var myTarget = (PlayableDirectorBinder)target;

			GUILayout.Space(10f);
			GUILayout.Label("Required Steps:");

			if (GUILayout.Button("Wire bindings!")) {
				myTarget.WireAndTest();
				ForceRepaintTrick(myTarget);
			}

			GUILayout.Label("Test and Utils:");

			if (GUILayout.Button("Search and bind scene objects")) {
				myTarget.SearchAndBindObjectsOnScene();
			}

			if (GUILayout.Button("Cleanup bindings references")) {
				myTarget.CleanupBindings();
			}

			if (GUILayout.Button("Reset track names with assigned components")) {
				myTarget.ResetOnlyAssignedTrackNames();
				ForceRepaintTrick(myTarget);
			}

			if (GUILayout.Button("Reset *ALL* track Names")) {
				var actionConfirmed = EditorUtility.DisplayDialog(
					"Reset all track names?",
					"Track names is where the info is retrieved to wire bindable assets. " +
					"You will have to 'Wire bindings' again to make the logic work, " +
					"but Empty tracks will be difficult to fix.",
					"Ok",
					"Cancel");

				if (actionConfirmed) {
					myTarget.ResetAllTrackNames();
					ForceRepaintTrick(myTarget);
				}
			}
		}

		// Weird and ugly way to reselect this gameObject and re paint the PlayableDirector editor component (DirectorEditor)
		// Tried other methods like Repaint, SetDirty, etc, but I was unable to update the names of the bindings.
		// So the only solution that worked was to unselect and select again this gameObject where most of the time
		// will also have attached PlayableDirector component
		private void ForceRepaintTrick(MonoBehaviour thisGameObject) {
			IEnumerator ReselectAfterAFrame() {
				Selection.activeGameObject = null;
				yield return new WaitForEndOfFrame();
				Selection.activeGameObject = thisGameObject.gameObject;
			}

			thisGameObject.StartCoroutine(ReselectAfterAFrame());
		}
	}
}

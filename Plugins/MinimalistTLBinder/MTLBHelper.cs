using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MinimalistTLBinder {
	public static class MTLBHelper {
		public static IEnumerable<BindableAsset> GetBindableAssetsOnScene() {
			return Resources.FindObjectsOfTypeAll<BindableAsset>().Where(ba => ba.gameObject.scene.isLoaded);
		}

		// track name example :`ActivationTrack t:(GameObject) => id:[bindElementName_324324]`
		private const string TRACK_NAME_TEMPLATE = "{0} t:({1}) => id:[{2}]";
		private static readonly Regex regex = new Regex(@"=> id:\[(.*)\]");

		public static string FormatTrackAssetName(string trackName, string bindType, string bindId) {
			// Cleanup bindType so instead of 'Unity.GameObject' is just 'GameObject'
			const char NAMESPACE_SEPARATOR = '.';
			var bindTypeShortened = bindType.Split(NAMESPACE_SEPARATOR).Last();

			return string.Format(TRACK_NAME_TEMPLATE, trackName, bindTypeShortened, bindId);
		}

		public static string ExtractBindableId(string label) {
			var match = regex.Match(label);

			var regexDidNotSuccess = !match.Success;
			var groupIdNotFound = match.Groups.Count <= 1 || !match.Groups[1].Success;

			if (regexDidNotSuccess || groupIdNotFound) {
				Debug.LogWarning($"Cannot extract Id from: [{label}] :( ");
				return null;
			}

			return match.Groups[1].Value;
		}
	}
}

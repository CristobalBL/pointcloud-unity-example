using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace GracesGames.Common.Scripts {

	// Utilities class defining re-usable methods

	public class Utilities : MonoBehaviour {

		// Finds and returns a game object by name or prints an error and return null
		public static GameObject FindGameObjectOrError(string objectName) {
			GameObject foundGameObject = GameObject.Find(objectName);
			if (foundGameObject == null) {
				Debug.LogError("Make sure " + objectName + " is present");
			}
			return foundGameObject;
		}

		// Tries to find a button by name and add an on click listener action to it
		// Returns the resulting button 
		public static GameObject FindButtonAndAddOnClickListener(string buttonName, UnityAction listenerAction) {
			GameObject button = FindGameObjectOrError(buttonName);
			button.GetComponent<Button>().onClick.AddListener(listenerAction);
			return button;
		}
	}
}
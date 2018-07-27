using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace GracesGames.SimpleFileBrowser.Scripts {
	/// <summary>
	/// Point cloud mesh script.
	/// Create random points and put it in a Mesh with Point Topology. The color depend of the vertical (Y) position of the point. 
	/// </summary>

	public class ControlVisualizer: MonoBehaviour {

		// Use the file browser prefab
		public GameObject FileBrowserPrefab;

		// Main camera 
		public GameObject camera;

		// Define a file extension
		public string[] FileExtensions;

		// Point Cloud Object
		private GameObject PointCloudObject;

		// File Browser
		private GameObject fileBrowserObject;

		private bool _pointcloudloaded = false;

		void Start () {

			GameObject uiCanvas = GameObject.Find("Canvas");
			if (uiCanvas == null) {
				Debug.LogError("Make sure there is a canvas GameObject present in the Hierarcy (Create UI/Canvas)");
			}

			OpenFileBrowser ();
		}

		void Update () {
			// Volver abrir File Browser
			if ( _pointcloudloaded &&  Input.GetKeyDown (KeyCode.Escape)){
				Debug.Log ("Back Button Pressed");

				// Destroy point cloud object 
				_pointcloudloaded = false;
				Destroy (PointCloudObject);

				// Re-open file browser
				OpenFileBrowser ();
			}
		}

		private void LoadPointCloudFromFile(string path){
			// load gameobject
			PointCloudObject = PC_OBJLoader.LoadOBJFile (path);

			// Set parameters
			PointCloudObject.name = "PointCloudMesh";
			//PointCloudObject.transform.position = new Vector3 (0, 0, 0);
			//PointCloudObject.transform.localScale = new Vector3 (20, 20, 20);

			// Set vertices color
			Vector2 maxmin = GetMaxAndMin ();
			SetPointCloudColor (maxmin [0], maxmin [1]);

			// Adjust camera position
			camera.transform.position = new Vector3(0.0f, maxmin[0] + 10.0f, 0.0f); 
		}

		/// <summary>
		/// Gets the max and minimum of current floorplan gameobject
		/// </summary>
		/// <returns>The max and minimum.</returns>
		Vector2 GetMaxAndMin(){
			Vector2 maxmin = new Vector2 (-1000.0f, 1000.0f);
			foreach (Transform child in PointCloudObject.transform) {

				GameObject subObject = child.gameObject;
				Vector3[] cvertices = subObject.GetComponent<MeshFilter>().mesh.vertices;

				for (int i = 0; i < cvertices.Length; ++i) {

					Vector3 current_vertice = cvertices [i];
					maxmin [0] = Mathf.Max (maxmin[0], current_vertice.y);
					maxmin [1] = Mathf.Min (maxmin[1], current_vertice.y);
				}
			}

			return maxmin;
		}

		/// <summary>
		/// Sets the color of the point cloud.
		/// </summary>
		/// <param name="max">Max.</param>
		/// <param name="min">Minimum.</param>
		void SetPointCloudColor(float max, float min){
			foreach (Transform child in PointCloudObject.transform) {

				GameObject subObject = child.gameObject;
				MeshFilter cmf = subObject.GetComponent<MeshFilter>();

				Vector3[] cvertices = cmf.mesh.vertices; 
				Color[] colors = new Color[cvertices.Length];
				for (int i = 0; i < cvertices.Length; ++i) {

					float value = 1.0f * ((cvertices [i].y) - min) / (max - min);

					colors [i] = new Color (0.0f, value, 0.0f, 1.0f);
				}
				cmf.mesh.colors = colors;
			} 
		}

		// Open a file browser to save and load files
		private void OpenFileBrowser() {
			// Create the file browser and name it
			fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
			fileBrowserObject.name = "FileBrowser";
			// Set the mode to save or load
			FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
			#if UNITY_EDITOR
			fileBrowserScript.SetupFileBrowser(ViewMode.Landscape);
			#else
			string directory = GetAndroidInternalFilesDir();
			fileBrowserScript.SetupFileBrowser(ViewMode.Landscape, directory);
			#endif
			fileBrowserScript.OpenFilePanel(FileExtensions);
			// Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
			fileBrowserScript.OnFileSelect += LoadFileUsingPath;
		}

		private void CloseFileBrowser() {
			FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
			fileBrowserScript.CloseFileBrowser ();
		}

		// Loads a file using a path
		private void LoadFileUsingPath(string path) {
			if (path.Length != 0) {
				CloseFileBrowser ();
				Debug.Log ("path -> " + path);
				LoadPointCloudFromFile (path);
				_pointcloudloaded = true;
			} else {
				Debug.Log("Invalid path given");
			}
		}

		//Find Android internal directories
		public static string GetAndroidInternalFilesDir()
		{
			string[] potentialDirectories = new string[]
			{
				"/mnt/sdcard",
				"/sdcard",
				"/storage/sdcard0",
				"/storage/sdcard1"
			};

			if(Application.platform == RuntimePlatform.Android)
			{
				for(int i = 0; i < potentialDirectories.Length; i++)
				{
					if(Directory.Exists(potentialDirectories[i]))
					{
						return potentialDirectories[i];
					}
				}
			}
			return "";
		}
	}
}

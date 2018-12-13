//-----------------------------------------------------------------------
// <summary>
// ControlFloorPlanScript.cs
// author: Cristobal Barrientos Low
// File developed for the master's thesis: Development of an application for the generation of floor plan based on the reconstruction of the real scene.
// Universidad Politécnica de Valencia, Spain
// </summary>
//-----------------------------------------------------------------------
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Control floor plan script.
/// 
/// This class allows to control the behavior of the generation stage of the building floor plan.
/// </summary>
namespace GracesGames.SimpleFileBrowser.Scripts {
	public class FileBrowserScript : MonoBehaviour {

		// Use the file browser prefab
		public GameObject fileBrowserPrefab;


		// Define a file extension
		public string[] FileExtensions;


		// File Browser Game Object
		private GameObject fileBrowserObject;


		private void Start(){
            OpenFileBrowser ();
		}

		// Open the file browser using boolean parameter so it can be called in GUI
		public void OpenFileBrowser() {

			GameObject uiCanvas = GameObject.Find("Canvas");
			if (uiCanvas == null) {
				Debug.LogError("Make sure there is a canvas GameObject present in the Hierarcy (Create UI/Canvas)");
			}

			OpenFileBrowser(FileBrowserMode.Load);
		}
        
        // Open the file browser using Load mode
        public void OpenFileBrowerLoadMode() {
            OpenFileBrowser(FileBrowserMode.Load);
        }

		// Open a file browser to save and load files
		private void OpenFileBrowser(FileBrowserMode fileBrowserMode) {
			// Create the file browser and name it
			fileBrowserObject = Instantiate(fileBrowserPrefab, transform);
			fileBrowserObject.name = "FileBrowser";
			// Set the mode to save or load
			FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
			fileBrowserScript.SetupFileBrowser(ViewMode.Landscape);
			if (fileBrowserMode == FileBrowserMode.Load) {
				fileBrowserScript.OpenFilePanel(FileExtensions);
				// Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
				fileBrowserScript.OnFileSelect += LoadFileUsingPath;
			}
		}

        // Loads a file using a path
        private void LoadFileUsingPath(string path)
        {
            if (path.Length != 0)
            {
                fileBrowserObject.SetActive(false);

                PointCloudLoader controlscript = gameObject.GetComponent<PointCloudLoader>();
                controlscript.enabled = true;
                controlscript.LoadObjectFromFile(path);
                
            }
            else
            {
                Debug.Log("Invalid path given");
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Back Button Pressed");
                SceneManager.LoadScene("PlaneReconstruction/Scenes/FloorPlanMainMenu");
            }
           
        }

    }
}

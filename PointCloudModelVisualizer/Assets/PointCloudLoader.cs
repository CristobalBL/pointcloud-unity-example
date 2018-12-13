//-----------------------------------------------------------------------
// <summary>
// PointCloudLoader.cs
// author: Cristobal Barrientos Low
// </summary>
//-----------------------------------------------------------------------
using UnityEngine;
using System.IO;
using GracesGames.SimpleFileBrowser.Scripts;
using System.Collections.Generic;
using System.Collections;

public class PointCloudLoader : MonoBehaviour {

    // Main Camera
    private Camera m_MainCamera;

    // GUI
    private float progress = 0;
    private new string guiText;
    private bool loaded = false;
    private bool updateVisual = false;
    
    private const string UI_FONT_SIZE = "<size=15>";
    private const float UI_LABEL_SIZE_X = 200.0f;
    private const float UI_LABEL_SIZE_Y = 25.0f;

    // PointCloud
    private string filename;
    private string filepath;
    private GameObject pointCloud;
    public Material matVertex;
    private Color defaultColor = Color.green; 
    

    public float scale = 1; // Scale Points ?
    public bool invertYZ = false; // Invert Y and Z
    private bool useColors = true;
    private float yfactor = 1.1f; // X-Y Draw Plane
    private float maxCameraPos = 0.0f; // Max Value of camera pos

    public int numPoints;
    public int numPointGroups;
    private int limitPoints = 65000;

    private Vector3[] points;
    private Color[] colors;
    private Vector3 minValue;

    // Max and Min for point cloud colorization
    private float max_h;
    private float min_h;

    public string Filepath
    {
        get
        {
            return filepath;
        }

        set
        {
            filepath = value;
        }
    }


    // Use this for initialization
    void Start () {
        
        m_MainCamera = Camera.main;
    }

    // Load file
    public void LoadObjectFromFile(string path)
    {
        Debug.Log("Loading Mesh ->" + path);
        // Set filepath
        filepath = path;

        // Get Filename
        filename = Path.GetFileName(filepath);

        loadPointCloud(filepath);
        
    }

    void loadPointCloud(string path)
    {
        // Check what file exists

        if (File.Exists(path))
        {
            StartCoroutine("loadOBJ", path);
        }
        else Debug.Log("File '" + path + "' could not be found");

    }

    /// <summary>
    /// Gets the max and minimum of current floorplan gameobject
    /// </summary>
    /// <returns>The max and minimum.</returns>
    Vector2 GetMaxAndMin()
    {
        Vector2 maxmin = new Vector2(-1000.0f, 1000.0f);
        foreach (Transform child in pointCloud.transform)
        {

            GameObject subObject = child.gameObject;
            Vector3[] cvertices = subObject.GetComponent<MeshFilter>().mesh.vertices;

            for (int i = 0; i < cvertices.Length; ++i)
            {

                Vector3 current_vertice = cvertices[i];
                maxmin[0] = Mathf.Max(maxmin[0], current_vertice.y);
                maxmin[1] = Mathf.Min(maxmin[1], current_vertice.y);
            }
        }

        return maxmin;
    }

    // Start Coroutine of reading the points from the OBJ file and creating the meshes
    IEnumerator loadOBJ(string dPath)
    {
        // Array data list
        List<Vector3> pointList = new List<Vector3>();
        List<Color> colorList = new List<Color>();

        // Read file
        StreamReader sr = new StreamReader(dPath);
        string line;
        string[] buffer;
        int n_lines = 0;
        int n_vertices = 0;
        int n_objects = 0;

        // Progress bar step
        int max_lines = 10000000;

        while ((line = sr.ReadLine()) != null)
        {
            buffer = line.Split();
            if (buffer[0] == "g")
            {
                n_objects += 1;
            }
            else if (buffer[0] == "v")
            {
                n_vertices += 1;
                pointList.Add(new Vector3(float.Parse(buffer[1]), float.Parse(buffer[2]), float.Parse(buffer[3])));
                if (buffer.Length > 6)
                    colorList.Add(new Color(float.Parse(buffer[4]), float.Parse(buffer[5]), float.Parse(buffer[6])));
                else
                    colorList.Add(defaultColor);
            }

            n_lines += 1;

            // GUI
            progress = n_lines * 1.0f / (max_lines - 1) * 1.0f;
            if (n_lines % Mathf.FloorToInt(max_lines / 200) == 0)
            {
                guiText = n_lines.ToString() + " lines have been read ... ";
                yield return null;
            }
            
        }

        Debug.Log("n lines: " + n_lines);
        Debug.Log("n vertices: " + n_vertices);
        Debug.Log("n objects: " + n_objects);

        numPoints = pointList.Count;
        points = new Vector3[numPoints];
        colors = new Color[numPoints];
        minValue = new Vector3();

        for (int i = 0; i < numPoints; i++)
        {
            Vector3 buffer_point = pointList[i];
            Color color = colorList[i];

            if (!invertYZ)
                points[i] = new Vector3(buffer_point[0] * scale, buffer_point[1] * scale, buffer_point[2] * scale);
            else
                points[i] = new Vector3(buffer_point[0] * scale, buffer_point[2] * scale, buffer_point[1] * scale);

            colors[i] = color;

            // Relocate Points near the origin
            //calculateMin(points[i]);

            // GUI
            progress = i * 1.0f / (numPoints - 1) * 1.0f;
            if (i % Mathf.FloorToInt(numPoints / 20) == 0)
            {
                guiText = i.ToString() + " out of " + numPoints.ToString() + " loaded";
                yield return null;
            }
        }


        // Instantiate Point Groups
        numPointGroups = Mathf.CeilToInt(numPoints * 1.0f / limitPoints * 1.0f);

        pointCloud = new GameObject(filename);

        for (int i = 0; i < numPointGroups - 1; i++)
        {
            InstantiateMesh(i, limitPoints);
            if (i % 10 == 0)
            {
                guiText = i.ToString() + " out of " + numPointGroups.ToString() + " PointGroups loaded";
                yield return null;
            }
        }
        InstantiateMesh(numPointGroups - 1, numPoints - (numPointGroups - 1) * limitPoints);


        loaded = true;
        updateVisual = true;
    }

    void InstantiateMesh(int meshInd, int nPoints)
    {
        // Create Mesh
        GameObject pointGroup = new GameObject(filename + meshInd);
        pointGroup.AddComponent<MeshFilter>();
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().material = new Material(Shader.Find("Custom/VertexColor"));

        pointGroup.GetComponent<MeshFilter>().mesh = CreateMesh(meshInd, nPoints, limitPoints);
        pointGroup.transform.parent = pointCloud.transform;
    }

    Mesh CreateMesh(int id, int nPoints, int limitPoints)
    {

        Mesh mesh = new Mesh();

        Vector3[] myPoints = new Vector3[nPoints];
        int[] indecies = new int[nPoints];
        Color[] myColors = new Color[nPoints];

        for (int i = 0; i < nPoints; ++i)
        {
            myPoints[i] = points[id * limitPoints + i] - minValue;
            indecies[i] = i;
            myColors[i] = useColors ? colors[id * limitPoints + i] : defaultColor;
        }
        
        mesh.vertices = myPoints;
        mesh.colors = myColors;
        mesh.SetIndices(indecies, MeshTopology.Points, 0);
        mesh.uv = new Vector2[nPoints];
        mesh.normals = new Vector3[nPoints];
       
        return mesh;
    }

    private Bounds CalculateLocalBounds()
    {
        Quaternion currentRotation = pointCloud.transform.rotation;
        pointCloud.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Bounds bounds = new Bounds(pointCloud.transform.position, Vector3.zero);

        foreach (Renderer renderer in pointCloud.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 localCenter = bounds.center - pointCloud.transform.position;
        bounds.center = localCenter;
        Debug.Log("The local bounds of this model is " + bounds);

        pointCloud.transform.rotation = currentRotation;

        return bounds;
    }

    private void SetOriginalColors()
    {
        int id = 0;
        foreach (Transform child in pointCloud.transform)
        {
            GameObject subObject = child.gameObject;
            MeshFilter cmf = subObject.GetComponent<MeshFilter>();

            Vector3[] cvertices = cmf.mesh.vertices;
            Color[] ccolors = new Color[cvertices.Length];
            for (int i = 0; i < cvertices.Length; ++i)
                    ccolors[i] = colors[id * limitPoints + i];
            
            cmf.mesh.colors = ccolors;

            id += 1;
        }
    }

    private void SetColorsByPos(float max, float min) {

        foreach (Transform child in pointCloud.transform)
        {
            GameObject subObject = child.gameObject;
            MeshFilter cmf = subObject.GetComponent<MeshFilter>();

            Vector3[] cvertices = cmf.mesh.vertices;
            Color[] colors = new Color[cvertices.Length];
            for (int i = 0; i < cvertices.Length; ++i)
            {
                float value = 1.0f * ((cvertices[i].y) - min) / (max - min);
                colors[i] = new Color(0.0f, value, 0.0f, 1.0f);
            }

            cmf.mesh.colors = colors;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Set camera pos text
        if (m_MainCamera.transform.position.y > maxCameraPos) {
            m_MainCamera.transform.position = new Vector3(m_MainCamera.transform.position.x, maxCameraPos, m_MainCamera.transform.position.z);
        }


        // Update colors of point cloud if camera is below a object height
        if (!useColors && m_MainCamera.transform.position.y < max_h)
        {
            SetColorsByPos(m_MainCamera.transform.position.y, min_h);
        }
        
        if (updateVisual) {

            // Calculate bounds of the mesh
            Bounds pointcloudbounds = CalculateLocalBounds();
            Vector3 pointcloudcenter = pointcloudbounds.center;
            Vector3 pointcloudextents = pointcloudbounds.extents;

            max_h = pointcloudcenter[1] + pointcloudextents[1];
            min_h = pointcloudcenter[1] - pointcloudextents[1];

            // Adjust camera position
            if (m_MainCamera != null)
            {
                // float vFOV = m_MainCamera.fieldOfView * Mathf.PI / 180;
                m_MainCamera.orthographicSize = Mathf.Max(pointcloudextents[0], pointcloudextents[2]);
                //float pos_camera_y = Mathf.Max(pointcloudextents[0], pointcloudextents[2]) / Mathf.Tan(vFOV / 2.0f);
                float pos_camera_y = max_h * 2.0f;
                float center_x = pointcloudcenter[0];
                float center_z = pointcloudcenter[2];
                maxCameraPos = pos_camera_y * yfactor;
                m_MainCamera.transform.position = new Vector3(center_x, maxCameraPos, center_z);
            }

            updateVisual = false;

        }

        // Volver abrir File Browser
        if (loaded && Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Back Button Pressed");

            // Destroy point cloud object 
            loaded = false;
            Destroy(pointCloud);

            // Re-open file browser
            FileBrowserScript fileBrowserScript = gameObject.GetComponent<FileBrowserScript>();
            fileBrowserScript.OpenFileBrowerLoadMode();

            // disable script
            enabled = false;
        }
    }

    void OnGUI()
    {

        if (!loaded)
        {
            GUI.BeginGroup(new Rect(Screen.width / 2 - 100, Screen.height / 2, 400.0f, 20));
            GUI.Box(new Rect(0, 0, 200.0f, 20.0f), guiText);
            GUI.Box(new Rect(0, 0, progress * 200.0f, 20), "");
            GUI.EndGroup();
        }
        else
        {
            int v_margin = 20;
            int label_xpos = 10;
            GUI.color = Color.white;

            // Main UI (left)
            if (GUI.Toggle(new Rect(label_xpos, v_margin, UI_LABEL_SIZE_X, UI_LABEL_SIZE_Y), useColors, UI_FONT_SIZE + "Use Colors" + "</size>") != useColors) {
                useColors = !useColors; // toggle manually

                if (useColors)
                    SetOriginalColors();
                else
                    SetColorsByPos(m_MainCamera.transform.position.y, min_h);
            }

        }
    }
}

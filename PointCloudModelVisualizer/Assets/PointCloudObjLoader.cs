/// <summary>
/// Object loader.
/// Load .obj file in a suitable form for Tango Dynamic Mesh save method 
/// Based in work "Runtime OBJ Loader" development for AAR04130 
/// </summary>
/// 
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PC_OBJLoader
{
	//structures
	struct OBJFace
	{
		public string meshName;
		public int[] indexes;
	}


	//functions
	#if UNITY_EDITOR
	[MenuItem("GameObject/Import From OBJ")]
	static void ObjLoadMenu()
	{
		string pth = UnityEditor.EditorUtility.OpenFilePanel("Import OBJ", "", "obj");
		if (!string.IsNullOrEmpty(pth))
		{
			System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
			s.Start();
			LoadOBJFile(pth);
			Debug.Log("OBJ load took " + s.ElapsedMilliseconds + "ms");
			s.Stop();
		}
	}
	#endif

	//ADDED
	public static Vector3 ParseVectorFromCMPS_MOD(string[] cmps)
	{

		float x = float.Parse(cmps[1]);
		float y = float.Parse(cmps[2]);
		float z = float.Parse(cmps[3]);

		return new Vector3 (x, y, z);

	}
	//ADDED

	public static Vector3 ParseVectorFromCMPS(string[] cmps)
	{

		float x = float.Parse (cmps [1]);
		float y = float.Parse (cmps [2]);
		if (cmps.Length == 4) {
			float z = float.Parse (cmps [3]);
			return new Vector3 (x, y, z);
		}
		return new Vector2 (x, y);
	}
	public static Color ParseColorFromCMPS(string[] cmps,float scalar = 1.0f)
	{
		float Kr = float.Parse(cmps[1]) * scalar ;
		float Kg = float.Parse(cmps[2]) * scalar;
		float Kb = float.Parse(cmps[3]) * scalar;
		return new Color(Kr, Kg, Kb);
	}
		
	public static GameObject LoadOBJFile(string fn)
	{

		string meshName = Path.GetFileNameWithoutExtension(fn);

		Debug.Log ("Mesh Name -> " + meshName);

		bool hasNormals = false;
		//OBJ LISTS
		List<Vector3> vertices = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		//UMESH LISTS
		List<Vector3> uvertices = new List<Vector3>();
		List<Vector3> unormals = new List<Vector3>();
		List<Vector2> uuvs = new List<Vector2>();
		//MESH CONSTRUCTION
		List<string> objectNames = new List<string>();
		Dictionary<string,int> hashtable = new Dictionary<string, int>();
		List<OBJFace> faceList = new List<OBJFace>();
		string cmesh = "default";
		//save this info for later
		//FileInfo OBJFileInfo = new FileInfo(fn);

		foreach (string ln in File.ReadAllLines(fn))
		{
			if (ln.Length > 0 && ln[0] != '#')
			{
				string l = ln.Trim().Replace("  "," ");
				string[] cmps = l.Split(' ');
				string data = l.Remove(0, l.IndexOf(' ') + 1);

				if (cmps[0] == "g" || cmps[0] == "o")
				{
					cmesh = data;
					if (!objectNames.Contains(cmesh))
					{
						objectNames.Add(cmesh);
					}
				}
				else if (cmps[0] == "v")
				{
					//VERTEX
					vertices.Add(ParseVectorFromCMPS_MOD(cmps));
				}
				else if (cmps[0] == "vn")
				{
					//VERTEX NORMAL
					normals.Add(ParseVectorFromCMPS(cmps));
				}
				else if (cmps[0] == "vt")
				{
					//VERTEX UV
					uvs.Add(ParseVectorFromCMPS(cmps));
				}
				else if (cmps[0] == "f")
				{
					int[] indexes = new int[cmps.Length - 1];
					for (int i = 1; i < cmps.Length ; i++)
					{
						string felement = cmps[i];
						int vertexIndex = -1;
						int normalIndex = -1;
						int uvIndex = -1;
						if (felement.Contains("//"))
						{
							//doubleslash, no UVS.
							string[] elementComps = felement.Split('/');
							vertexIndex = int.Parse(elementComps[0]) - 1;
							normalIndex = int.Parse(elementComps[2]) - 1;
						}
						else if (felement.Count(x => x == '/') == 2)
						{
							//contains everything
							string[] elementComps = felement.Split('/');
							vertexIndex = int.Parse(elementComps[0]) - 1;
							uvIndex = int.Parse(elementComps[1]) - 1;
							normalIndex = int.Parse(elementComps[2]) - 1;
						}
						else if (!felement.Contains("/"))
						{
							//just vertex inedx
							vertexIndex = int.Parse(felement) - 1;
						}
						else
						{
							//vertex and uv
							string[] elementComps = felement.Split('/');
							vertexIndex = int.Parse(elementComps[0]) - 1;
							uvIndex = int.Parse(elementComps[1]) - 1;
						}
						string hashEntry = vertexIndex + "|" + normalIndex + "|" +uvIndex;
						if (hashtable.ContainsKey(hashEntry))
						{
							indexes[i - 1] = hashtable[hashEntry];
						}
						else
						{
							//create a new hash entry
							indexes[i - 1] = hashtable.Count;
							hashtable[hashEntry] = hashtable.Count;
							uvertices.Add(vertices[vertexIndex]);
							if (normalIndex < 0 || (normalIndex > (normals.Count - 1)))
							{
								unormals.Add(Vector3.zero);
							}
							else
							{
								hasNormals = true;
								unormals.Add(normals[normalIndex]);
							}
							if (uvIndex < 0 || (uvIndex > (uvs.Count - 1)))
							{
								uuvs.Add(Vector2.zero);
							}
							else
							{
								uuvs.Add(uvs[uvIndex]);
							}

						}
					}
					if (indexes.Length < 5 && indexes.Length >= 3)
					{
						OBJFace f1 = new OBJFace();
						f1.indexes = new int[] { indexes[0], indexes[1], indexes[2] };
						f1.meshName = cmesh;
						faceList.Add(f1);
						if (indexes.Length > 3)
						{

							OBJFace f2 = new OBJFace();
							f2.meshName = cmesh;
							f2.indexes = new int[] { indexes[2], indexes[3], indexes[0] };
							faceList.Add(f2);
						}
					}
				}
			}
		}

		if (objectNames.Count == 0)
			objectNames.Add("default");

		//build objects
		GameObject parentObject = new GameObject(meshName);

		foreach (string obj in objectNames)
		{
			GameObject subObject = new GameObject(obj);
			subObject.transform.parent = parentObject.transform;
			subObject.transform.localScale = new Vector3(1, 1, 1);
			//Create mesh
			Mesh m = new Mesh();
			m.name = obj;
			//LISTS FOR REORDERING
			List<Vector3> processedVertices = new List<Vector3>();
			Dictionary<int,int> remapTable = new Dictionary<int, int>();
			//POPULATE MESH
			//List<string> meshMaterialNames = new List<string>();

			OBJFace[] ofaces = faceList.Where(x =>  x.meshName == obj).ToArray();

			if (ofaces.Length > 0)
			{
				int[] indexes = new int[0];
				foreach (OBJFace f in ofaces)
				{
					int l = indexes.Length;
					System.Array.Resize(ref indexes, l + f.indexes.Length);
					System.Array.Copy(f.indexes, 0, indexes, l, f.indexes.Length);
				}

				//meshMaterialNames.Add(obj);
				for (int i = 0; i < indexes.Length; i++)
				{
					int idx = indexes[i];
					//build remap table
					if (remapTable.ContainsKey(idx))
					{
						//ezpz
						indexes[i] = remapTable[idx];
					}
					else
					{
						processedVertices.Add(uvertices[idx]);
						remapTable[idx] = processedVertices.Count - 1;
						indexes[i] = remapTable[idx];
					}
				}

			}

			//apply stuff
			Vector3[] points = processedVertices.ToArray();
			int[] indecies = new int[points.Length];
			for (int i = 0; i < points.Length; i++)
			{
				indecies [i] = i;
			}

			m.vertices = points;
			m.SetIndices(indecies, MeshTopology.Points,0);

			if (!hasNormals)
			{
				m.RecalculateNormals();   
			}
			m.RecalculateBounds();
			;


			MeshFilter mf = subObject.AddComponent<MeshFilter>();
			MeshRenderer mr = subObject.AddComponent<MeshRenderer>();

			mr.material = new Material(Shader.Find("Custom/VertexColor"));
			mf.mesh = m;
		}
			
		return parentObject;
	}
}

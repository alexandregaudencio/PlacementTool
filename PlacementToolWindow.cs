using UnityEditor;
using UnityEngine;

namespace PlacementTool
{
    public class PlacementToolWindow : EditorWindow
    {
        public static PlacementToolWindow window;
        static SceneView.OnSceneFunc onSceneGUIFunc;

        private bool enabled = false;
        private bool applyXNormalRotation = false;
        private bool applyYNormalRotation = false;
        private bool applyZNormalRotation = false;
        private Vector3Int rotationOffset = Vector3Int.zero;

        private bool customOffset = false;
        private Vector3 buildPos;
        private Vector3 offSet;
        private bool instantiatePrefab = false;
        private Vector2 scrollPos;
        private string newGroupName;
        private GameObject selectedGroup;
        private GameObject currentGameObject;
        private GameObject newSelectedGameObject;
        private int indexname = 0;

        [MenuItem("Tools/Placement Tool")]
        public static void ShowWindow()
        {

            window = EditorWindow.GetWindow<PlacementToolWindow>(false, "Placement Fool");
        }

        void OnEnable()
        {
            onSceneGUIFunc = this.OnSceneGUI;
            SceneView.onSceneGUIDelegate += onSceneGUIFunc;
        }

        void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= onSceneGUIFunc;
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (enabled)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                if (Selection.activeObject != null)
                {
                    if (Selection.activeGameObject != null && Selection.activeTransform == null)
                    {
                        newSelectedGameObject = Selection.activeGameObject;
                    }

                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit))
                        {
                            buildPos = hit.point + offSet;

                            if (Selection.activeGameObject != null && Selection.activeTransform == null)
                            {
                                instantiatePrefab = true;
                            }
                            if (Selection.activeTransform != null)
                            {
                                instantiatePrefab = false;
                            }

                            if (instantiatePrefab == true)
                            {
                                AddSelections(buildPos, hit);
                            }

                            if (instantiatePrefab == false)
                            {
                                WarnUser();
                            }
                        }
                    }
                }
            }
        }

        private void AddSelections(Vector3 buildPos, RaycastHit clickedObject)
        {
            GameObject[] gameObjects = Selection.gameObjects;
            int indexRandom = Random.Range(0, gameObjects.Length);
            GameObject prefab = PrefabUtility.InstantiatePrefab(gameObjects[indexRandom]) as GameObject;

            Vector3 newPos = new Vector3(buildPos.x, buildPos.y, buildPos.z);

            prefab.transform.position = newPos;

            //APPLYING ROTATION
            Vector3 surfaceNormalResult = Vector3.zero;
            if (applyXNormalRotation) surfaceNormalResult.x = clickedObject.normal.x;
            if (applyYNormalRotation) surfaceNormalResult.y = clickedObject.normal.y;
            if (applyZNormalRotation) surfaceNormalResult.z = clickedObject.normal.z;
            //prefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormalResult) * prefab.transform.rotation;
            prefab.transform.rotation = Quaternion.FromToRotation(Vector3.up, surfaceNormalResult) * prefab.transform.rotation * Quaternion.Euler(rotationOffset);
            //TODO: REVIEW TO APPLY Rotation to camera.
            https://chatgpt.com/c/6726806b-f5c0-800e-9b1b-688f61009f54

            if (selectedGroup != null)
            {
                prefab.transform.parent = selectedGroup.transform;
            }

            indexname++;

            prefab.name = string.Format("{0}_{1}", prefab.name, indexname);

            Undo.RegisterCreatedObjectUndo(prefab, "Added " + prefab.name + " to Scene");
        }

        private void WarnUser()
        {
            Debug.Log("You have selected an object in the scene! Please use prefabs or gameobjects from your project window or disable the Point Click Placement Tool");
        }

        void OnGUI()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);


            if (enabled == false)
            {

                if (GUILayout.Button("Enable", GUILayout.Height(30)))
                {
                    enabled = true;
                }
                GUI.backgroundColor = Color.red;
                GUILayout.Space(20);
                GUILayout.Box("", GUILayout.ExpandWidth(true));
            }
            else if (enabled == true)
            {


                if (GUILayout.Button("Disable", GUILayout.Height(30)))
                {
                    enabled = false;
                }
                GUI.backgroundColor = Color.green;

                GUILayout.Space(20);
                if (customOffset == false)
                {


                    if (GUILayout.Button("Enable Custom Offset"))
                    {
                        customOffset = true;
                    }

                    offSet = Vector3.zero;
                }
                else if (customOffset == true)
                {

                    if (GUILayout.Button("Disable Custom Offset"))
                    {
                        customOffset = false;
                    }


                    offSet = EditorGUILayout.Vector3Field("Set Offset", offSet);
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();



                newGroupName = EditorGUILayout.TextField("New Group Name:", newGroupName);

                if (GUILayout.Button("Create New Group"))
                {
                    GameObject newObj = new GameObject();
                    selectedGroup = newObj;
                    newObj.name = newGroupName;
                    newGroupName = "";
                    GUIUtility.keyboardControl = 0;

                    Undo.RegisterCreatedObjectUndo(newObj, "Created New Group " + newObj.name);
                }

                EditorGUILayout.Space();

                if (selectedGroup != null)
                {
                    EditorGUILayout.LabelField("Selected Group: " + selectedGroup.name);
                }


                if (GUILayout.Button("Select Group"))
                {
                    if (Selection.activeGameObject != null)// && Selection.activeTransform != null)
                    {
                        selectedGroup = Selection.activeGameObject;
                    }
                }

                if (GUILayout.Button("Clear Selected Group"))
                {
                    selectedGroup = null;
                }


                EditorGUILayout.Space(20);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Apply Surface Rotation");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("X", GUILayout.Width(10));
                applyXNormalRotation = EditorGUILayout.Toggle(applyXNormalRotation, GUILayout.Width(20));
                EditorGUILayout.LabelField("Y", GUILayout.Width(10));
                applyYNormalRotation = EditorGUILayout.Toggle(applyYNormalRotation, GUILayout.Width(20));
                EditorGUILayout.LabelField("Z", GUILayout.Width(10));
                applyZNormalRotation = EditorGUILayout.Toggle(applyZNormalRotation, GUILayout.Width(20));

                EditorGUILayout.EndHorizontal();


                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Rotation offset:", EditorStyles.boldLabel, GUILayout.Width(100));
                EditorGUIUtility.labelWidth = 10;
                rotationOffset.x = EditorGUILayout.IntField("X", rotationOffset.x, GUILayout.Width(50));
                rotationOffset.y = EditorGUILayout.IntField("Y", rotationOffset.y, GUILayout.Width(50));
                rotationOffset.z = EditorGUILayout.IntField("Z", rotationOffset.z, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(20);
                EditorGUIUtility.labelWidth = 0;

                EditorGUILayout.LabelField("Selected Objects:");
                if (IsObjectSelectionOnProject())
                {
                    float boxWidth = 128;
                    foreach (GameObject gameObject in UnityEditor.Selection.gameObjects)
                    {
                        EditorGUILayout.LabelField(gameObject.name, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(boxWidth));
                        GUILayout.Box(AssetPreview.GetAssetPreview(gameObject), GUILayout.Width(boxWidth), GUILayout.Height(boxWidth));
                    }
                }
            }


            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        private bool IsObjectSelectionOnProject()
        {
            if (Selection.activeObject != null && Selection.activeGameObject != null)
            {
                if (Selection.activeTransform == null)
                {
                    return true;
                }
            }
            return false;
        }


    }

}
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SecondLaw.EditorTools
{
    public static class SecondLawProjectBuilder
    {
        [MenuItem("Second Law/Create Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 5.2f;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SecondLawDemo.unity");
            EditorUtility.DisplayDialog("Second Law", "Created Assets/Scenes/SecondLawDemo.unity. Press Play to run the scripted demo.", "OK");
        }

        [MenuItem("Second Law/Reset Demo Progress")]
        public static void ResetDemoProgress()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            EditorUtility.DisplayDialog("Second Law", "Local demo progress and cached letters were reset.", "OK");
        }
    }
}

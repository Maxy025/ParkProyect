using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab : EditorWindow
{
    GameObject prefabToUse;

    [MenuItem("Tools/Reemplazar por Prefab")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceWithPrefab>("Reemplazar por Prefab");
    }

    void OnGUI()
    {
        GUILayout.Label("Reemplazar objetos seleccionados por Prefab", EditorStyles.boldLabel);
        prefabToUse = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabToUse, typeof(GameObject), false);

        if (GUILayout.Button("Reemplazar"))
        {
            if (prefabToUse == null)
            {
                Debug.LogError("¡Selecciona un prefab primero!");
                return;
            }

            foreach (GameObject obj in Selection.gameObjects)
            {
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToUse);
                if (newObj != null)
                {
                    newObj.transform.position = obj.transform.position;
                    newObj.transform.rotation = obj.transform.rotation;
                    newObj.transform.localScale = obj.transform.localScale;
                    newObj.transform.parent = obj.transform.parent;
                    newObj.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());

                    Undo.RegisterCreatedObjectUndo(newObj, "Reemplazar con Prefab");
                    Undo.DestroyObjectImmediate(obj);
                }
            }
            Debug.Log("¡Reemplazo completado con éxito!");
        }
    }
}
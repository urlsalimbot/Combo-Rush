#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Singletons;

public class SetupGameManagers : MonoBehaviour
{
    [MenuItem("Tools/Combo Rush/Setup Game Managers")]
    static void Setup()
    {
        // Create StateMaster
        var stateMasterObj = new GameObject("StateMaster");
        stateMasterObj.AddComponent<StateMaster>();
        Debug.Log("Created StateMaster GameObject");

        // Create GameMaster
        var gameMasterObj = new GameObject("GameMaster");
        gameMasterObj.AddComponent<GameMaster>();
        Debug.Log("Created GameMaster GameObject");

        // Select StateMaster for easy reference assignment
        Selection.activeGameObject = stateMasterObj;
        
        Debug.Log("Setup complete! Assign references in the Inspector.");
    }
}
#endif

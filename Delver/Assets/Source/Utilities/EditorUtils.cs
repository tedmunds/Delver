using UnityEngine;
using UnityEditor;

public class EditorUtils : MonoBehaviour {

#if UNITY_EDITOR
    protected void Update() {
        if(Input.GetKeyDown(KeyCode.F1)) {
            EditorApplication.isPlaying = false;
        }  	
	}
#endif

}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    void Update()
    {
        
//        transform.LookAt(Camera.main.transform);       
        transform.LookAt(SceneView.GetAllSceneCameras()[0].transform);       
    }
}

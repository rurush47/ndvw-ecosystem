using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof (FieldOfView))]
public class FieldOfViewEditor : Editor
{
    void OnSceneGUI(){
        FieldOfView fow = (FieldOfView)target;

        Handles.color = Color.white;
        Handles.DrawWireArc(fow.transform.position,Vector3.up, Vector3.forward,360,fow.viewRadius);
        Vector3 viewAngleA = fow.DirFromAngle(-fow.viewAngle/2,false);
        Vector3 viewAngleB = fow.DirFromAngle(fow.viewAngle/2,false);

        Handles.DrawLine(fow.transform.position,fow.transform.position + viewAngleA * fow.viewRadius);
        Handles.DrawLine(fow.transform.position,fow.transform.position + viewAngleB * fow.viewRadius);

        Handles.color = Color.red;
        foreach (Transform visiblePreys in fow.visiblePreys)
        {
            if(!fow.transform.gameObject) return;
            Handles.DrawLine(fow.transform.position,visiblePreys.position);
        }

        Handles.color = Color.blue;
        foreach (Transform visiblePredators in fow.visiblePredators)
        {
            if(!fow.transform.gameObject) return;
            Handles.DrawLine(fow.transform.position,visiblePredators.position);
        }

        
        Handles.color = Color.green;
        foreach (Transform visiblePreyFoods in fow.visiblePreyFoods)
        {
            if(!fow.transform.gameObject) return;
            Handles.DrawLine(fow.transform.position,visiblePreyFoods.position);
        }
        
        Handles.color = Color.yellow;
        foreach (Transform visibleWaterPoints in fow.visibleWaterPoints)
        {
            if(!fow.transform.gameObject) return;
            Handles.DrawLine(fow.transform.position,visibleWaterPoints.position);
        }
    }
}


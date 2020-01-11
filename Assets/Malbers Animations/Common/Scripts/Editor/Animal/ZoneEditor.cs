using UnityEngine;
using UnityEditor;

namespace MalbersAnimations.Controller
{
    [CustomEditor(typeof(Zone))]
   // [CanEditMultipleObjects]
    public class ZoneEditor : Editor
    {
        private Zone M;

        SerializedProperty
            HeadOnly, stateAction, HeadName, zoneType, stateID, modeID, ID, ActionID, auto, AutomaticDisabled, stanceAction, statModifier, stanceID;
            


        MonoScript script;
        private void OnEnable()
        {
            M = ((Zone)target);
            script = MonoScript.FromMonoBehaviour((MonoBehaviour)target);

            HeadOnly = serializedObject.FindProperty("HeadOnly");
            HeadName = serializedObject.FindProperty("HeadName");
            zoneType = serializedObject.FindProperty("zoneType");
            stateID = serializedObject.FindProperty("stateID");
            stateAction = serializedObject.FindProperty("stateAction");
            stanceAction = serializedObject.FindProperty("stanceAction");
            modeID = serializedObject.FindProperty("modeID");
            stanceID = serializedObject.FindProperty("stanceID");
            ID = serializedObject.FindProperty("ID");
            ActionID = serializedObject.FindProperty("ActionID");
            auto = serializedObject.FindProperty("automatic");
            AutomaticDisabled = serializedObject.FindProperty("AutomaticDisabled");

            statModifier = serializedObject.FindProperty("StatModifier");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription("Zone Activate |States| or |Modes| on an Animal");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);
            {
                MalbersEditor.DrawScript(script);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    //zoneType.intValue = (int)(ZoneType)EditorGUILayout.EnumPopup(new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"), (ZoneType)zoneType.intValue);
                    EditorGUILayout.PropertyField(zoneType, new GUIContent("Zone Type", "Choose between a Mode or a State for the Zone"));

                    ZoneType zone = (ZoneType)zoneType.intValue;


                switch (zone)
                {
                    case ZoneType.Mode:
                        EditorGUILayout.PropertyField(modeID, new GUIContent("Mode ID", "Which Mode to Set when entering the Zone"));

                        serializedObject.ApplyModifiedProperties();


                        if (M.modeID != null && M.modeID == 4)
                        {
                            EditorGUILayout.PropertyField(ActionID, new GUIContent("Action ID", "Which Action to Set when entering the Zone"));

                            if (ActionID.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("Please Select an Action ID", MessageType.Error);
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(ID, new GUIContent("Ability ID", "Which Ability to Set when entering the Zone"));
                            if (ActionID.objectReferenceValue == null)
                            {
                                EditorGUILayout.HelpBox("Please Select an Ability ID", MessageType.Error);
                            }
                        }
                        break;
                    case ZoneType.State:
                        EditorGUILayout.PropertyField(stateID, new GUIContent("State ID", "Which State will Activate when entering the Zone"));
                        EditorGUILayout.PropertyField(stateAction, new GUIContent("Action", "Set what action for State the animal will apply when entering the zone"));
                        if (stateID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an State ID", MessageType.Error);
                        }
                        break;
                    case ZoneType.Stance:
                        EditorGUILayout.PropertyField(stanceID, new GUIContent("Stance ID", "Which Stance will Activate when entering the Zone"));
                        EditorGUILayout.PropertyField(stanceAction, new GUIContent("Action", "Set what action for stance the animal will apply when entering the zone"));
                        if (stanceID.objectReferenceValue == null)
                        {
                            EditorGUILayout.HelpBox("Please Select an Stance ID", MessageType.Error);
                        }
                        break;
                    default:
                        break;
                }

   

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUILayout.PropertyField(HeadOnly, new GUIContent("Head Only", "Activate when the Head Bone the Zone.\nThe Head Bone needs a collider and be Named Head"));
                    if (HeadOnly.boolValue)
                    {
                        EditorGUILayout.PropertyField(HeadName, new GUIContent("Head Name", "Name for the Head Bone"));
                    }
                }
                EditorGUILayout.EndVertical();

                if (zone == ZoneType.Mode)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        EditorGUILayout.PropertyField(auto, new GUIContent("Automatic", "As soon as the animal enters the zone play the action"));

                        if (auto.boolValue)
                        {
                            EditorGUILayout.PropertyField(AutomaticDisabled, new GUIContent("Disabled", "if true the Trigger will be disabled for this value in seconds"));

                            if (AutomaticDisabled.floatValue < 0) AutomaticDisabled.floatValue = 0;
                        }
                    }
                    EditorGUILayout.EndVertical();
                } 


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(statModifier,true);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                M.EditorShowEvents = EditorGUILayout.Foldout(M.EditorShowEvents, "Events");
                EditorGUI.indentLevel--;

                if (M.EditorShowEvents)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnEnter"), new GUIContent("On Animal Enter"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnExit"), new GUIContent("On Animal Exit"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnZoneActivation"), new GUIContent("On Zone Active"));
                }
                EditorGUILayout.EndVertical();


                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                M.EditorAI = EditorGUILayout.Foldout(M.EditorAI, "AI");
                if (M.EditorAI)
                {
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("stoppingDistance"), new GUIContent("Stopping Distance"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("waitTime"), new GUIContent("Wait Time", "Wait Time after the Action ended"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("pointType"), new GUIContent("Zone Type", "Which type is this action zone"));
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("nextTargets"), new GUIContent("Next Targets", "Posible Targets to go after finishing the Action"), true);
                    EditorGUI.indentLevel--;
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();

                //      base.OnInspectorGUI();
            }
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Zone Inspector");
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();

        }
    }
}
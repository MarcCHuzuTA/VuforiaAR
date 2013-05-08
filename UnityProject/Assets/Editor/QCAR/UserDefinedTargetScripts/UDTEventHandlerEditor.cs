/*==============================================================================
Copyright (c) 2012-2013 QUALCOMM Austria Research Center GmbH.
All Rights Reserved.
Qualcomm Confidential and Proprietary
==============================================================================*/

using UnityEditor;

/// <summary>
/// This editor class renders the custom inspector for the UserDefinedTargetEventHandler MonoBehaviour
/// </summary>
[CustomEditor(typeof(UserDefinedTargetEventHandler))]
public class UDTEventHandlerEditor : Editor
{
    #region UNITY_EDITOR_METHODS

    // Draws a custom UI for the user defined target event handler inspector
    public override void OnInspectorGUI()
    {
        UserDefinedTargetEventHandler udtehb = (UserDefinedTargetEventHandler)target;

        EditorGUILayout.HelpBox("Here you can set the ImageTargetBehaviour from the scene that will be used to augment user created targets.", MessageType.Info);
        bool allowSceneObjects = !EditorUtility.IsPersistent(target);
        udtehb.ImageTargetTemplate = (ImageTargetBehaviour)EditorGUILayout.ObjectField("Image Target Template",
                                                    udtehb.ImageTargetTemplate, typeof(ImageTargetBehaviour), allowSceneObjects);
    }

    #endregion // UNITY_EDITOR_METHODS
}
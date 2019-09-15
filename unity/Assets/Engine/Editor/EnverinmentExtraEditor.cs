using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CFUtilPoolLib;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [CustomEditor(typeof(EnverinmentExtra))]
    public class EnverinmentExtraEditor : BaseEditor<EnverinmentExtra>
    {
        //lighting
        private SerializedProperty roleLight0;
        private SerializedProperty roleLight1;
        //debugShadow
        private SerializedProperty fitWorldSpace;
        private SerializedProperty lookTarget;
        private SerializedProperty shadowMapLevel;
        private SerializedProperty shadowBound;
        private SerializedProperty drawShadowLighing;

        private SerializedProperty drawType;
        private SerializedProperty quadIndex;
        private SerializedProperty showObjects;
        private SerializedProperty drawLightBox;
        private SerializedProperty isDebugLayer;

        private SerializedProperty debugMode;
        private SerializedProperty debugDisplayType;
        private SerializedParameter splitAngle;
        private SerializedProperty splitPos;
        private float splitLeft = -1;
        private float splitRight = 1;

        public void OnEnable()
        {
            roleLight0 = FindProperty(x => x.roleLight0);
            roleLight1 = FindProperty(x => x.roleLight1);

            fitWorldSpace = FindProperty(x => x.fitWorldSpace);
            shadowMapLevel = FindProperty(x => x.shadowMapLevel);
            shadowBound = FindProperty(x => x.shadowBound);
            lookTarget = FindProperty(x => x.lookTarget);
            drawShadowLighing = FindProperty(x => x.drawShadowLighing);

            drawType = FindProperty(x => x.drawType);
            quadIndex = FindProperty(x => x.quadIndex);
            showObjects = FindProperty(x => x.showObjects);
            drawLightBox = FindProperty(x => x.drawLightBox);

            isDebugLayer = FindProperty(x => x.isDebugLayer);
            debugMode = FindProperty(x => x.debugContext.debugMode);
            debugDisplayType = FindProperty(x => x.debugContext.debugDisplayType);
            splitAngle = FindParameter(x => x.debugContext.splitAngle);
            splitPos = FindProperty(x => x.debugContext.splitPos);

            AssetsConfig.RefreshShaderDebugNames();
        }

        void LightInstpetorGui(SerializedProperty lightSP, TransformRotationGUIWrapper wrapper, string name)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(lightSP);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "lightSP" + name);
            }

            Light light = lightSP.objectReferenceValue as Light;
            if (light != null)
            {
                EditorGUI.BeginChangeCheck();
                Color color = EditorGUILayout.ColorField("Color", light.color);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Color" + name);
                    light.color = color;
                }
                EditorGUI.BeginChangeCheck();
                float intensity = EditorGUILayout.Slider("Intensity", light.intensity, 0, 10.0f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Intensity" + name);
                    light.intensity = intensity;
                }
                EditorGUI.BeginChangeCheck();
                if (wrapper != null)
                    wrapper.OnGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "rot" + name);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EnverinmentExtra ee = target as EnverinmentExtra;
            if (ee != null)
            {
                ee.lightingFolder = EditorGUILayout.Foldout(ee.lightingFolder, "Lighting");
                if (ee.lightingFolder)
                {
                    ToolsUtility.BeginGroup("RoleLight");
                    LightInstpetorGui(roleLight0, ee.roleLight0Rot, "RoleLight0");
                    LightInstpetorGui(roleLight1, ee.roleLight1Rot, "RoleLight1");
                    ToolsUtility.EndGroup();
                }

                if (ToolsUtility.BeginFolderGroup("Shadow", ref ee.shadowFolder))
                {
                    shadowMapLevel.floatValue = EditorGUILayout.Slider("ShadowMap Scale", shadowMapLevel.floatValue, 0, 1);

                    if (ee.shadowMapLevel == 0 || ee.shadowMap == null)
                    {
                        EditorGUILayout.ObjectField(ee.shadowMap, typeof(RenderTexture), false);
                    }
                    else
                    {
                        float size = 256 * ee.shadowMapLevel;
                        GUILayout.Space(size + 20);
                        Rect r = GUILayoutUtility.GetLastRect();
                        r.y += 10;
                        r.width = size;
                        r.height = size;
                        EditorGUI.DrawPreviewTexture(r, ee.shadowMap);

                    }
                    EditorGUILayout.PropertyField(shadowBound);
                    EditorGUILayout.PropertyField(lookTarget);
                    EditorGUILayout.PropertyField(fitWorldSpace);
                    EditorGUILayout.PropertyField(drawShadowLighing);
                    ToolsUtility.EndFolderGroup();
                }

                if (ToolsUtility.BeginFolderGroup("Debug", ref ee.debugFolder))
                {
                    EditorGUILayout.PropertyField(drawType);
                    EditorGUILayout.PropertyField(drawLightBox);
                    if (drawLightBox.boolValue)
                    {
                        EditorGUI.BeginChangeCheck();
                    }
                    EditorGUI.BeginChangeCheck();
                    if (EditorGUI.EndChangeCheck())
                    {
                        debugMode.intValue = 0;
                        debugDisplayType.intValue = 0;
                        splitAngle.value.floatValue = 0;
                        splitPos.floatValue = 0;
                    }
                    string[] debugNames = null;
                    bool refreshDebug = false;

                    if (GUILayout.Button("Refresh"))
                    {
                        refreshDebug = true;
                    }
                    debugNames = AssetsConfig.shaderDebugNames;
                    ee.debugContext.shaderID = EnverinmentExtra.debugShaderIDS;


                    if (debugNames != null)
                    {
                        EditorGUI.BeginChangeCheck();
                        debugMode.intValue = EditorGUILayout.Popup("DebugMode", debugMode.intValue, debugNames);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "GlobalDebugMode");
                            ee.debugContext.modeModify = true;
                        }
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(debugDisplayType);
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "DebugDisplayType");
                            ee.debugContext.typeModify = true;
                        }
                        if (debugDisplayType.intValue == (int)DebugDisplayType.Split)
                        {
                            EditorGUI.BeginChangeCheck();
                            PropertyField(splitAngle);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(target, "SplitAngle");
                                ee.debugContext.angleModify = true;
                            }
                            EditorGUI.BeginChangeCheck();
                            splitPos.floatValue = EditorGUILayout.Slider("SplitPos", splitPos.floatValue, splitLeft, splitRight);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(target, "SplitPos");
                                ee.debugContext.posModify = true;
                            }
                        }

                    }
                    ToolsUtility.EndFolderGroup();
                    if (refreshDebug)
                    {
                        AssetsConfig.RefreshShaderDebugNames();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }



    }
}
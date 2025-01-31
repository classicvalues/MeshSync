using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.MeshSync;
using UnityEditor;
using UnityEditor.UIElements;
using Constants = Unity.MeshSync.Editor.MeshSyncEditorConstants;

namespace Unity.MeshSync.Editor {

internal class MeshSyncPlayerConfigSection {

    internal class Contents {

        public static readonly GUIContent Visibility = EditorGUIUtility.TrTextContent("Visibility");
        public static readonly GUIContent Transform  = EditorGUIUtility.TrTextContent("Transform");
        public static readonly GUIContent Cameras = EditorGUIUtility.TrTextContent("Cameras");
        public static readonly GUIContent Lights  = EditorGUIUtility.TrTextContent("Lights");
        public static readonly GUIContent Meshes = EditorGUIUtility.TrTextContent("Meshes");
        public static readonly GUIContent UpdateMeshColliders = EditorGUIUtility.TrTextContent("Update mesh colliders");
        public static readonly GUIContent Materials = EditorGUIUtility.TrTextContent("Materials");
        public static readonly GUIContent FindMaterialsFromAssetDatabase = EditorGUIUtility.TrTextContent("Find materials from asset database");

        public static readonly GUIContent AnimationInterpolation = EditorGUIUtility.TrTextContent("Animation interpolation");
        public static readonly GUIContent KeyframeReduction  = EditorGUIUtility.TrTextContent("Keyframe reduction");
        public static readonly GUIContent ReductionThreshold = EditorGUIUtility.TrTextContent("Reduction threshold");
        public static readonly GUIContent ReductionEraseFlatCurves = EditorGUIUtility.TrTextContent("Reduction erase flat curves");
        public static readonly GUIContent ZUpCorrection = EditorGUIUtility.TrTextContent("Z-Up correction");

        
        public static readonly GUIContent SyncMaterialList = EditorGUIUtility.TrTextContent("Sync material list");
        public static readonly GUIContent ProgressiveDisplay = EditorGUIUtility.TrTextContent("Progressive display");
        public static readonly GUIContent Logging = EditorGUIUtility.TrTextContent("Logging");
        public static readonly GUIContent Profiling = EditorGUIUtility.TrTextContent("Profiling");

        public static readonly GUIContent TweakTimeScale = EditorGUIUtility.TrTextContent("Time scale");
        public static readonly GUIContent TweakTimeOffset = EditorGUIUtility.TrTextContent("Time offset");
        public static readonly GUIContent TweakDropStep = EditorGUIUtility.TrTextContent("Drop step");
        public static readonly GUIContent TweakReductionThreshold = EditorGUIUtility.TrTextContent("Reduction threshold");
        public static readonly GUIContent TweakEraseFlatCurves = EditorGUIUtility.TrTextContent("Erase flat curves");

        public static readonly GUIContent TimelineSnapToFrame = EditorGUIUtility.TrTextContent("Snap To Frame");
        
    }

//----------------------------------------------------------------------------------------------------------------------
    
    internal MeshSyncPlayerConfigSection(MeshSyncPlayerType playerType) {
        m_playerType = playerType;
        
        
    }
    
//----------------------------------------------------------------------------------------------------------------------        
    internal void Setup(VisualElement parent) {
        m_playerConfigUIElements = new List<VisualElement>();
        
        TemplateContainer containerInstance = InstantiateContainer(m_playerType);
        
        //Templates
        VisualTreeAsset fieldTemplate = UIElementsEditorUtility.LoadVisualTreeAsset(Constants.PROJECT_SETTINGS_FIELD_TEMPLATE_PATH);
    
        //Add server port	            	          
        Foldout syncSettingsFoldout = containerInstance.Query<Foldout>("SyncSettingsFoldout").First();

        //Sync	           
        m_syncVisibilityToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, Contents.Visibility,
            (MeshSyncPlayerConfig config, bool newValue) => { config.SyncVisibility = newValue; }
        );
        m_syncTransformToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, Contents.Transform,
            (MeshSyncPlayerConfig config, bool newValue) => { config.SyncTransform = newValue; }
        );
        m_syncCamerasToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, Contents.Cameras,
            (MeshSyncPlayerConfig config, bool newValue) => { config.SyncCameras = newValue; }
        );
        m_syncLightsToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, Contents.Lights,
            (MeshSyncPlayerConfig config, bool newValue) => { config.SyncLights = newValue; }
        );
        m_syncMeshesToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, Contents.Meshes,
            (MeshSyncPlayerConfig config, bool newValue) => { config.SyncMeshes = newValue; }
        );
        m_updateMeshCollidersToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, 
            Contents.UpdateMeshColliders,
            (MeshSyncPlayerConfig config, bool newValue) => { config.UpdateMeshColliders = newValue; }
        );
        m_syncMaterialsToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, Contents.Materials,
            (MeshSyncPlayerConfig config, bool newValue) => { config.SyncMaterials = newValue; }
        );
        m_findMaterialFromAssetsToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, syncSettingsFoldout, 
            Contents.FindMaterialsFromAssetDatabase,
            (MeshSyncPlayerConfig config, bool newValue) => { config.FindMaterialFromAssets = newValue; }
        );

        //import
        Foldout importSettingsFoldout = containerInstance.Query<Foldout>("ImportSettingsFoldout").First();

        m_animationInterpolationPopup = AddPlayerConfigPopupField(fieldTemplate, importSettingsFoldout, 
            Contents.AnimationInterpolation, m_animationInterpolationEnums,
            (MeshSyncPlayerConfig config, int newValue) => { config.AnimationInterpolation = newValue; }
        );
        m_keyframeReductionToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, importSettingsFoldout, 
            Contents.KeyframeReduction,
            (MeshSyncPlayerConfig config, bool newValue) => { config.KeyframeReduction = newValue; }
        );
        m_reductionThresholdField = AddPlayerConfigField<FloatField, float>(fieldTemplate, importSettingsFoldout, 
            Contents.ReductionThreshold,
            (MeshSyncPlayerConfig config, float newValue) => { config.ReductionThreshold = newValue; }
        );
        m_reductionEraseFlatCurves = AddPlayerConfigField<Toggle,bool>(fieldTemplate, importSettingsFoldout, 
            Contents.ReductionEraseFlatCurves,
            (MeshSyncPlayerConfig config, bool newValue) => { config.ReductionEraseFlatCurves = newValue; }
        );
        m_zUpCorrectionPopup = AddPlayerConfigPopupField(fieldTemplate, importSettingsFoldout, 
            Contents.ZUpCorrection, m_zUpCorrectionEnums,
            (MeshSyncPlayerConfig config, int newValue) => { config.ZUpCorrection = newValue; }
        );
        
        //Misc 
        Foldout miscSettingsFoldout = containerInstance.Query<Foldout>("MiscSettingsFoldout").First();
        m_syncMaterialListToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, miscSettingsFoldout, 
            Contents.SyncMaterialList,
            (MeshSyncPlayerConfig config, bool newValue) => { config.SyncMaterialList = newValue; }
        );
        m_progressiveDisplayToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, miscSettingsFoldout, 
            Contents.ProgressiveDisplay,
            (MeshSyncPlayerConfig config, bool newValue) => { config.ProgressiveDisplay = newValue; }
        );
        m_loggingToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, miscSettingsFoldout, 
            Contents.Logging,
            (MeshSyncPlayerConfig config, bool newValue) => { config.Logging = newValue; }
        );
        m_profilingToggle = AddPlayerConfigField<Toggle,bool>(fieldTemplate, miscSettingsFoldout, 
            Contents.Profiling,
            (MeshSyncPlayerConfig config, bool newValue) => { config.Profiling = newValue; }
        );
        
        //Animation Tweak
        Foldout animationTweakSettingsFoldout = containerInstance.Query<Foldout>("AnimationTweakSettingsFoldout").First();
        m_animationTweakTimeScaleField = AddPlayerConfigField<FloatField, float>(fieldTemplate, animationTweakSettingsFoldout, 
            Contents.TweakTimeScale,
            (MeshSyncPlayerConfig config, float newValue) => {
                config.GetAnimationTweakSettings().TimeScale = newValue;
            }
        );
        m_animationTweakTimeOffsetField = AddPlayerConfigField<FloatField, float>(fieldTemplate, animationTweakSettingsFoldout, 
            Contents.TweakTimeOffset,
            (MeshSyncPlayerConfig config, float newValue) => {
                config.GetAnimationTweakSettings().TimeOffset = newValue;
            }
        );
        m_animationTweakDropStepField = AddPlayerConfigField<IntegerField, int>(fieldTemplate, animationTweakSettingsFoldout, 
            Contents.TweakDropStep,
            (MeshSyncPlayerConfig config, int newValue) => {
                config.GetAnimationTweakSettings().DropStep = newValue;
            }
        );
        m_animationTweakReductionThresholdField = AddPlayerConfigField<FloatField, float>(fieldTemplate, animationTweakSettingsFoldout, 
            Contents.TweakReductionThreshold,
            (MeshSyncPlayerConfig config, float newValue) => {
                config.GetAnimationTweakSettings().ReductionThreshold = newValue;
            }
        );
        m_animationTweakEraseFlatCurvesToggle = AddPlayerConfigField<Toggle, bool>(fieldTemplate, animationTweakSettingsFoldout, 
            Contents.TweakEraseFlatCurves,
            (MeshSyncPlayerConfig config, bool newValue) => {
                config.GetAnimationTweakSettings().EraseFlatCurves = newValue;
            }
        );
        
        bool isSceneCachePlayerConfig = (m_playerType == MeshSyncPlayerType.CACHE_PLAYER);
        if (isSceneCachePlayerConfig) {
            Foldout timelineSettingsFoldout = containerInstance.Query<Foldout>("TimelineSettingsFoldout").First();	    
            m_timelineSnapToFramePopup = AddPlayerConfigPopupField(fieldTemplate, timelineSettingsFoldout, 
                Contents.TimelineSnapToFrame, m_snapToFrameEnums,
                (SceneCachePlayerConfig config, int newValue) => { config.TimelineSnapToFrame = newValue;}
            );
            
        }
        
        //Update the values in each UI elements
        if (isSceneCachePlayerConfig) {
            UpdateSceneCacheUIElements();
        } else {
            UpdateServerUIElements();
        }
        
        parent.Add(containerInstance);
    }

    
//----------------------------------------------------------------------------------------------------------------------	

    //Support Toggle, FloatField, etc
    private F AddPlayerConfigField<F,V>(VisualTreeAsset template, VisualElement parent, GUIContent content,
        Action<MeshSyncPlayerConfig,V> onValueChanged) where F: VisualElement,INotifyValueChanged<V>, new()  
    {

        TemplateContainer templateInstance = template.CloneTree();
        VisualElement fieldContainer = templateInstance.Query<VisualElement>("FieldContainer").First();
//		F field = templateInstance.Query<F>().First();
        Label label = templateInstance.Query<Label>().First();
        label.text = content.text;
        label.tooltip = content.tooltip;
        
        F field = new F();
        field.AddToClassList("general-settings-field");
        field.RegisterValueChangedCallback((ChangeEvent<V> changeEvent) => {

            MeshSyncPlayerConfig config = field.userData as MeshSyncPlayerConfig;
            if (null == config) {
                Debug.LogError("[MeshSync] Field doesn't have the correct user data");
                return;
            }
            
            onValueChanged(config, changeEvent.newValue);
            MeshSyncProjectSettings.GetOrCreateSettings().SaveSettings();
        });		
        
        fieldContainer.Add(field);
        parent.Add(templateInstance);
        m_playerConfigUIElements.Add(field);		
        return field;
    }
    
//----------------------------------------------------------------------------------------------------------------------	
    private PopupField<T> AddPlayerConfigPopupField<T,UserDataType>(VisualTreeAsset template, VisualElement parent, GUIContent content,
        List<T> options, Action<UserDataType,int> onValueChanged) where UserDataType: class
    {

        TemplateContainer templateInstance = template.CloneTree();
        VisualElement fieldContainer = templateInstance.Query<VisualElement>("FieldContainer").First();
        PopupField<T> popupField = new PopupField<T>(options,options[0]);
        popupField.AddToClassList("general-settings-field");
        
        Label label = templateInstance.Query<Label>().First();
        label.text    = content.text;
        label.tooltip = content.tooltip;
        popupField.RegisterValueChangedCallback( ( ChangeEvent<T> changeEvent)  => {
        
            UserDataType config = popupField.userData as UserDataType;
            if (null == config) {
                Debug.LogError("[MeshSync] Toggle doesn't have the correct user data");
                return;
            }
            
            onValueChanged(config, popupField.index);
            MeshSyncProjectSettings.GetOrCreateSettings().SaveSettings();
        });
                
        fieldContainer.Add(popupField);
        parent.Add(templateInstance);
        m_playerConfigUIElements.Add(popupField);		
        return popupField;
    }
    

//----------------------------------------------------------------------------------------------------------------------

    private void UpdateServerUIElements() {
        MeshSyncPlayerConfig config = MeshSyncProjectSettings.GetOrCreateSettings().GetDefaultServerConfig();	
        UpdateCommonUIElements(config);		
        SetupUIElementUserData(config);		
    }
    
    private void UpdateSceneCacheUIElements() {
        SceneCachePlayerConfig config = MeshSyncProjectSettings.GetOrCreateSettings().GetDefaultSceneCachePlayerConfig();	
        UpdateCommonUIElements(config);		
        m_timelineSnapToFramePopup.SetValueWithoutNotify(m_snapToFrameEnums[config.TimelineSnapToFrame]);
        SetupUIElementUserData(config);
    }

    private void UpdateCommonUIElements(MeshSyncPlayerConfig config) {
        //sync
        m_syncVisibilityToggle.SetValueWithoutNotify(config.SyncVisibility);
        m_syncTransformToggle.SetValueWithoutNotify(config.SyncTransform);
        m_syncCamerasToggle.SetValueWithoutNotify(config.SyncCameras);
        m_syncLightsToggle.SetValueWithoutNotify(config.SyncLights);
        m_syncMeshesToggle.SetValueWithoutNotify(config.SyncMeshes);
        m_updateMeshCollidersToggle.SetValueWithoutNotify(config.UpdateMeshColliders);
        m_syncMaterialsToggle.SetValueWithoutNotify(config.SyncMaterials);
        m_findMaterialFromAssetsToggle.SetValueWithoutNotify(config.FindMaterialFromAssets);

        //Import
        m_animationInterpolationPopup.SetValueWithoutNotify(m_animationInterpolationEnums[config.AnimationInterpolation]);
        m_keyframeReductionToggle.SetValueWithoutNotify(config.KeyframeReduction);
        m_reductionThresholdField.SetValueWithoutNotify(config.ReductionThreshold);
        m_reductionEraseFlatCurves.SetValueWithoutNotify(config.ReductionEraseFlatCurves);
        m_zUpCorrectionPopup.SetValueWithoutNotify(m_zUpCorrectionEnums[config.ZUpCorrection]);

        //Misc
        m_syncMaterialListToggle.SetValueWithoutNotify(config.SyncMaterialList);
        m_progressiveDisplayToggle.SetValueWithoutNotify(config.ProgressiveDisplay);
        m_loggingToggle.SetValueWithoutNotify(config.Logging);
        m_profilingToggle.SetValueWithoutNotify(config.Profiling);

        //Animation Tweak
        AnimationTweakSettings animationTweakSettings = config.GetAnimationTweakSettings();
        m_animationTweakTimeScaleField.SetValueWithoutNotify(animationTweakSettings.TimeScale);
        m_animationTweakTimeOffsetField.SetValueWithoutNotify(animationTweakSettings.TimeOffset);
        m_animationTweakDropStepField.SetValueWithoutNotify(animationTweakSettings.DropStep);
        m_animationTweakReductionThresholdField.SetValueWithoutNotify(animationTweakSettings.ReductionThreshold);
        m_animationTweakEraseFlatCurvesToggle.SetValueWithoutNotify(animationTweakSettings.EraseFlatCurves);
        
    }

    private void SetupUIElementUserData(MeshSyncPlayerConfig config) {
        foreach (VisualElement uiElement in m_playerConfigUIElements) {
            uiElement.userData = config;
        }		
    }
    
//----------------------------------------------------------------------------------------------------------------------

    // [TODO-sin: 2021-9-9] Move to FIU
    static List<string> GetEnumInspectorNames(Type t) {
        List<string> ret = new List<string>();
        foreach (MemberInfo mi in t.GetMembers( BindingFlags.Static | BindingFlags.Public)) {
            InspectorNameAttribute inspectorNameAttribute = (InspectorNameAttribute) Attribute.GetCustomAttribute(mi, typeof(InspectorNameAttribute));
            if (null == inspectorNameAttribute) {
                ret.Add(mi.Name);
                continue;
            }
            
            ret.Add(inspectorNameAttribute.displayName);			
        }

        return ret;
    }

    private static TemplateContainer InstantiateContainer(MeshSyncPlayerType playerType) {

        VisualTreeAsset container = null;
        switch (playerType) {
            case MeshSyncPlayerType.SERVER: 
                container = UIElementsEditorUtility.LoadVisualTreeAsset(Constants.SERVER_CONFIG_CONTAINER_PATH);
                break; 
            case MeshSyncPlayerType.CACHE_PLAYER: 
                container = UIElementsEditorUtility.LoadVisualTreeAsset(Constants.SCENE_CACHE_PLAYER_CONFIG_CONTAINER_PATH);
                break;
            default : 
                Assert.Fail();
                break;
        }
        
        return container.CloneTree();		
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    //Sync Settings
    private Toggle m_syncVisibilityToggle;
    private Toggle m_syncTransformToggle;
    private Toggle m_syncCamerasToggle;
    private Toggle m_syncLightsToggle;
    private Toggle m_syncMeshesToggle;
    private Toggle m_updateMeshCollidersToggle;
    private Toggle m_syncMaterialsToggle;
    private Toggle m_findMaterialFromAssetsToggle;
    
    //Import Settings
    private PopupField<string> m_animationInterpolationPopup;
    private Toggle m_keyframeReductionToggle;
    private FloatField m_reductionThresholdField;
    private Toggle m_reductionEraseFlatCurves;
    private PopupField<string> m_zUpCorrectionPopup;
    
    //Misc Settings
    private Toggle m_syncMaterialListToggle;
    private Toggle m_progressiveDisplayToggle;
    private Toggle m_loggingToggle;
    private Toggle m_profilingToggle;
    
    //AnimationTweak Settings
    private FloatField   m_animationTweakTimeScaleField;
    private FloatField   m_animationTweakTimeOffsetField;
    private IntegerField m_animationTweakDropStepField;
    private FloatField   m_animationTweakReductionThresholdField;
    private Toggle       m_animationTweakEraseFlatCurvesToggle;
    
    //Timeline
    private PopupField<string> m_timelineSnapToFramePopup;
    
    
    private readonly MeshSyncPlayerType m_playerType;
    private List<VisualElement> m_playerConfigUIElements;

    private readonly List<string> m_animationInterpolationEnums = new List<string>(Enum.GetNames( typeof( InterpolationMode )));
    private readonly List<string> m_zUpCorrectionEnums = new List<string>(Enum.GetNames( typeof( ZUpCorrectionMode )));

    private readonly List<string> m_snapToFrameEnums = GetEnumInspectorNames(typeof(SnapToFrame));
    
}

} //end namespace 

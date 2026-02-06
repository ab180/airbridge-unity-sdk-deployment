#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

internal class AirbridgeDataToggleSection
{
    private readonly string _title;
    private readonly AirbridgeData.Variant _variant;
    
    public bool Expanded = false;

    private readonly AirbridgeArrayDataHandler _customDomainHandler =
        new AirbridgeArrayTextDataHandler(
            label: AirbridgeEditorConstant.CustomDomain.CustomDomainLabel,
            elementLabel: AirbridgeEditorConstant.CustomDomain.CustomDomainElementLabel
        );

    private readonly AirbridgeArrayDataHandler _trackingBlocklistHandler =
        new AirbridgeArrayDropdownDataHandler(
            label: AirbridgeEditorConstant.BlockList.TrackingBlocklistLabel,
            options: AirbridgeTrackingBlocklistExtension.Values
        );
    
    public AirbridgeDataToggleSection(string title, AirbridgeData.Variant variant)
    {
        _title = title;
        _variant = variant;
    }
    
    public bool Foldout()
    {
        return EditorGUILayout.Foldout(Expanded, _title, true);
    }

    public void Draw()
    {
        AirbridgeScriptableObject airbridgeScriptableObject = (
            AirbridgeScriptableObject.GetInstance<AirbridgeData>(AirbridgeData.GetAssetName(_variant))
        );
        SerializedObject serializedObject = airbridgeScriptableObject.GetSerializedObject();

        float prevLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 300;

        EditorGUILayout.BeginVertical();

        EditorGUI.indentLevel++;

        if (DrawIsActive(serializedObject) && Expanded)
        {
            DrawAirbridgeData(serializedObject);
        }
        else if (Expanded)
        {
            EditorGUILayout.HelpBox(
                $"To configure these settings, first enable ‘Use {_title} Airbridge Settings’ above.",
                MessageType.Warning
            );
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();

        EditorGUIUtility.labelWidth = prevLabelWidth;

        EditorGUILayout.Space();

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            airbridgeScriptableObject.Save();
        }
    }

    private bool DrawIsActive(SerializedObject serializedObject)
    {
        SerializedProperty isActiveProperty = serializedObject.FindProperty("isActive");
        if (_variant == AirbridgeData.Variant.Default)
        {
            isActiveProperty.boolValue = true;
        }
        else
        {
            EditorGUILayout.PropertyField(
                isActiveProperty,
                new GUIContent($"Use {_title} Airbridge Settings"),
                new GUILayoutOption[] { }
            );
        }

        return isActiveProperty.boolValue;
    }

    private void DrawAirbridgeData(SerializedObject serializedObject)
    {
        SerializedProperty appNameProperty = serializedObject.FindProperty("appName");
        EditorGUILayout.PropertyField(appNameProperty, new GUILayoutOption[] { });
        
        SerializedProperty appTokenProperty = serializedObject.FindProperty("appToken");
        EditorGUILayout.PropertyField(appTokenProperty, new GUILayoutOption[] { });

        SerializedProperty logLevel = serializedObject.FindProperty("logLevel");
        logLevel.intValue = EditorGUILayout.Popup("Log Level", logLevel.intValue, AirbridgeLogLevelExtension.Levels);
        
        SerializedProperty iOSURISchemeProperty = serializedObject.FindProperty("iOSURIScheme");
        EditorGUILayout.PropertyField(iOSURISchemeProperty, new GUIContent("iOS URI Scheme"), new GUILayoutOption[] { });
        
        SerializedProperty androidURISchemeProperty = serializedObject.FindProperty("androidURIScheme");
        EditorGUILayout.PropertyField(androidURISchemeProperty, new GUILayoutOption[] { });

        SerializedProperty customDomainProperty = serializedObject.FindProperty("customDomainList");
        _customDomainHandler.SetProperty(customDomainProperty);
        _customDomainHandler.Draw();

        SerializedProperty sessionTimeoutSecondsProperty = serializedObject.FindProperty("sessionTimeoutSeconds");
        EditorGUILayout.PropertyField(sessionTimeoutSecondsProperty, new GUILayoutOption[] { });

        SerializedProperty userInfoHashEnabledProperty = serializedObject.FindProperty("userInfoHashEnabled");
        EditorGUILayout.PropertyField(userInfoHashEnabledProperty, new GUILayoutOption[] { });

        SerializedProperty locationCollectionEnabledProperty = serializedObject.FindProperty("locationCollectionEnabled");
        EditorGUILayout.PropertyField(locationCollectionEnabledProperty, new GUILayoutOption[] { });

        SerializedProperty trackAirbridgeLinkOnlyProperty = serializedObject.FindProperty("trackAirbridgeLinkOnly");
        EditorGUILayout.PropertyField(trackAirbridgeLinkOnlyProperty, new GUILayoutOption[] { });

        SerializedProperty autoStartTrackingEnabledProperty = serializedObject.FindProperty("autoStartTrackingEnabled");
        EditorGUILayout.PropertyField(autoStartTrackingEnabledProperty, new GUILayoutOption[] { });

        SerializedProperty facebookDeferredAppLinkEnabledProperty = serializedObject.FindProperty("facebookDeferredAppLinkEnabled");
        EditorGUILayout.PropertyField(facebookDeferredAppLinkEnabledProperty, new GUILayoutOption[] { });

        SerializedProperty iOSTrackingAuthorizeTimeoutSecondsProperty = serializedObject.FindProperty("iOSTrackingAuthorizeTimeoutSeconds");
        EditorGUILayout.PropertyField(iOSTrackingAuthorizeTimeoutSecondsProperty, new GUIContent("iOS Tracking Authorize Timeout Seconds"), new GUILayoutOption[] { });

        SerializedProperty sdkSignatureSecretIDProperty = serializedObject.FindProperty("sdkSignatureSecretID");
        EditorGUILayout.PropertyField(sdkSignatureSecretIDProperty, new GUIContent("SDK Signature Secret ID"), new GUILayoutOption[] { });
        
        SerializedProperty sdkSignatureSecretProperty = serializedObject.FindProperty("sdkSignatureSecret");
        EditorGUILayout.PropertyField(sdkSignatureSecretProperty, new GUIContent("SDK Signature Secret"), new GUILayoutOption[] { });
        
        SerializedProperty trackInSessionLifeCycleEventEnabledProperty = serializedObject.FindProperty("trackInSessionLifeCycleEventEnabled");
        EditorGUILayout.PropertyField(trackInSessionLifeCycleEventEnabledProperty, new GUILayoutOption[] { });
        
        SerializedProperty pauseEventTransmitOnBackgroundEnabledProperty = serializedObject.FindProperty("pauseEventTransmitOnBackgroundEnabled");
        EditorGUILayout.PropertyField(pauseEventTransmitOnBackgroundEnabledProperty, new GUILayoutOption[] { });
        
        SerializedProperty resetEventBufferEnabledProperty = serializedObject.FindProperty("resetEventBufferEnabled");
        EditorGUILayout.PropertyField(resetEventBufferEnabledProperty, new GUIContent("Clear Event Buffer On Initialize Enabled"), new GUILayoutOption[] { });
  
        SerializedProperty sdkEnabledProperty = serializedObject.FindProperty("sdkEnabled");
        EditorGUILayout.PropertyField(sdkEnabledProperty, new GUIContent("SDK Enabled"), new GUILayoutOption[] { });
        
        SerializedProperty appMarketIdentifierProperty = serializedObject.FindProperty("appMarketIdentifier");
        EditorGUILayout.PropertyField(appMarketIdentifierProperty, new GUILayoutOption[] { });
        
        SerializedProperty eventMaximumBufferCountProperty = serializedObject.FindProperty("eventMaximumBufferCount");
        EditorGUILayout.PropertyField(eventMaximumBufferCountProperty, new GUIContent("Event Buffer Count Limit"), new GUILayoutOption[] { });
        
        SerializedProperty eventMaximumBufferSizeProperty = serializedObject.FindProperty("eventMaximumBufferSize");
        EditorGUILayout.PropertyField(eventMaximumBufferSizeProperty, new GUIContent("Event Buffer Size Limit In Gibibyte"), new GUILayoutOption[] { });
  
        SerializedProperty eventTransmitIntervalSecondsProperty = serializedObject.FindProperty("eventTransmitIntervalSeconds");
        EditorGUILayout.PropertyField(eventTransmitIntervalSecondsProperty, new GUILayoutOption[] { });
        
        SerializedProperty facebookAppIdProperty = serializedObject.FindProperty("facebookAppId");
        EditorGUILayout.PropertyField(facebookAppIdProperty, new GUIContent("Meta Install Referrer (Facebook App ID)"), new GUILayoutOption[] { });
        
        SerializedProperty isHandleAirbridgeDeeplinkOnlyProperty = serializedObject.FindProperty("isHandleAirbridgeDeeplinkOnly");
        EditorGUILayout.PropertyField(isHandleAirbridgeDeeplinkOnlyProperty, new GUIContent("Is Handle Airbridge Deeplink Only"), new GUILayoutOption[] { });
        
        SerializedProperty inAppPurchaseEnvironment = serializedObject.FindProperty("inAppPurchaseEnvironment");
        inAppPurchaseEnvironment.intValue = EditorGUILayout.Popup(
            label: "In-App Purchase Environment",
            selectedIndex: inAppPurchaseEnvironment.intValue,
            displayedOptions: AirbridgeInAppPurchaseEnvironmentExtension.Environments
        );
        
        SerializedProperty collectTCFDataEnabledProperty = serializedObject.FindProperty("collectTCFDataEnabled");
        EditorGUILayout.PropertyField(collectTCFDataEnabledProperty, new GUILayoutOption[] { });

        SerializedProperty trackingBlocklistProperty = serializedObject.FindProperty("trackingBlocklist");
        _trackingBlocklistHandler.SetProperty(trackingBlocklistProperty);
        _trackingBlocklistHandler.Draw();
        
        SerializedProperty calculateSKAdNetworkByServer = serializedObject.FindProperty("calculateSKAdNetworkByServer");
        EditorGUILayout.PropertyField(calculateSKAdNetworkByServer, new GUILayoutOption[] { });
    }
}

#endif

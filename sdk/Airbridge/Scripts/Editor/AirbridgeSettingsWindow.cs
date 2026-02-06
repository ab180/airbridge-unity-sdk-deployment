#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

internal class AirbridgeSettingsWindow : EditorWindow
{
    private Vector2 _scrollPos;
    private List<AirbridgeDataToggleSection> _sections;

    [MenuItem("Airbridge/Airbridge Settings")]
    public static void ShowWindow()
    {
        EditorWindow window = GetWindow<AirbridgeSettingsWindow>("Airbridge Settings");

        // Set minimum size
        float minWidth = 900;
        float minHeight = 900;
        window.minSize = new Vector2(minWidth, minHeight);
    }

    private void OnEnable()
    {
        _sections = new List<AirbridgeDataToggleSection>
        {
            new AirbridgeDataToggleSection("Default", AirbridgeData.Variant.Default) { Expanded = true },
            new AirbridgeDataToggleSection("Development", AirbridgeData.Variant.Dev),
            new AirbridgeDataToggleSection("Production", AirbridgeData.Variant.Prod),
        };
    }

    private void OnGUI()
    {
        _scrollPos = EditorGUILayout.BeginScrollView(
            _scrollPos,
            GUILayout.Width(position.width),
            GUILayout.Height(position.height)
        );

        DrawSections(_sections);

        EditorGUILayout.EndScrollView();
    }

    private void DrawSections(List<AirbridgeDataToggleSection> sections)
    {
        sections.ForEachWithIndex((section, index) => DrawSection(sections, section, index));
    }

    private void DrawSection(List<AirbridgeDataToggleSection> sections, AirbridgeDataToggleSection section, int index)
    {
        var newExpanded = section.Foldout();
        var isNewlyExpanded = newExpanded && !section.Expanded;

        if (isNewlyExpanded)
        {
            sections.ForEachWithIndex((other, otherIndex) => other.Expanded = (otherIndex == index));
        }
        else
        {
            section.Expanded = newExpanded;
        }

        section.Draw();
    }
}

#endif
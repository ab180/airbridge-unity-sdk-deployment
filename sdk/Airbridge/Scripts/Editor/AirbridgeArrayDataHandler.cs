#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

internal abstract class AirbridgeArrayDataHandler
{
    protected readonly string Label;
    protected readonly string ElementLabel;

    protected abstract string Element { get; }
    protected abstract void DrawElement();
    protected abstract void ResetElement();

    public Action<string> AddAction;
    public Action<string> RemoveAction;

    private SerializedProperty _property;

    // Since the IMGUI is a stateless mode,
    // you must cache the ScrollView's scroll position to preserve the scrolling state.
    private Vector2 _scrollPosition;

    protected AirbridgeArrayDataHandler(string label, string elementLabel)
    {
        Label = label;
        ElementLabel = elementLabel;
    }

    public void SetProperty(SerializedProperty property)
    {
        _property = property;
    }

    private List<string> GetList() =>
        Enumerable.Range(0, _property.arraySize)
            .Select(i => _property.GetArrayElementAtIndex(i).stringValue)
            .ToList();


    private string GetElementLabel(string action)
    {
        return string.IsNullOrEmpty(ElementLabel) ? action : $"{action} {ElementLabel}";
    }

    protected virtual void DrawLabel()
    {
        EditorGUILayout.LabelField(Label, GUILayout.Width(300));
    }

    public void Draw()
    {
        if (_property == null)
        {
            Debug.LogError(
                $"[Airbridge] Before calling the Draw() function, set the property through calling the SetProperty() function. (at <{Label}>)");
            return;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawLabel();
            DrawElement();

            if (GUILayout.Button(
                    GetElementLabel("Add"),
                    EditorStyles.toolbarButton,
                    GUILayout.Width(120)))
            {
                Add(Element);
                ResetElement();
            }
        }

        DrawList();
    }

    private void DrawList()
    {
        var list = GetList();
        int i = 0;
        if (list.Count > 5)
        {
            // Drawing the scroll view
            using (
                var scrollViewScope = new EditorGUILayout.ScrollViewScope(
                    _scrollPosition,
                    GUILayout.MinHeight(105),
                    GUILayout.MaxHeight(105)
                )
            )
            {
                _scrollPosition = scrollViewScope.scrollPosition;
                foreach (var element in list) DrawListItem(i++, element);
            }
        }
        else
        {
            foreach (var element in list) DrawListItem(i++, element);
        }
    }

    private void DrawListItem(int i, string element)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            // Add padding space
            GUILayout.Space(300);

            GUILayout.Label((i + 1).ToString(), GUILayout.Width(20));
            GUILayout.Label(element);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(GetElementLabel("Remove"))) Remove(element);
        }
    }

    private void Add(string element)
    {
        if (string.IsNullOrEmpty(element)) return;

        // 중복되는 값이 존재하는 경우 무시한다.
        if (GetList().Contains(element)) return;

        int newIndex = _property.arraySize;
        _property.InsertArrayElementAtIndex(newIndex);
        _property.GetArrayElementAtIndex(newIndex).stringValue = element;

        AddAction?.Invoke(element);
    }

    private void Remove(string element)
    {
        if (string.IsNullOrEmpty(element)) return;

        int index = GetList().IndexOf(element);
        if (index < 0) return;

        _property.DeleteArrayElementAtIndex(index);

        RemoveAction?.Invoke(element);
    }
}

internal class AirbridgeArrayTextDataHandler : AirbridgeArrayDataHandler
{
    private string _input = string.Empty;

    public AirbridgeArrayTextDataHandler(string label, string elementLabel = null)
        : base(label, elementLabel)
    {
    }

    protected override string Element => _input?.Trim();

    protected override void DrawElement()
    {
        // Only draw TextField when Event type is safe
        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout ||
            Event.current.isMouse || Event.current.isKey)
        {
            _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));
        }
    }

    protected override void ResetElement()
    {
        _input = string.Empty;
    }
}

internal class AirbridgeArrayDropdownDataHandler : AirbridgeArrayDataHandler
{
    private readonly string[] _options;

    private int _selectedIndex = 0;

    public AirbridgeArrayDropdownDataHandler(string label, string[] options, string elementLabel = null)
        : base(label, elementLabel)
    {
        _options = options;
    }

    protected override string Element => _options[_selectedIndex];

    protected override void DrawElement()
    {
        _selectedIndex = EditorGUILayout.Popup(Label, _selectedIndex, _options);
    }

    protected override void ResetElement()
    {
        _selectedIndex = 0;
    }

    protected override void DrawLabel()
    {
        // Label is already drawn inside DrawElement, so implementation is omitted.
    }
}

#endif
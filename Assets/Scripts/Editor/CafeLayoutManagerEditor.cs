using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CafeLayoutManager))]
public class CafeLayoutManagerEditor : Editor
{
    private static readonly Color ErrorTint = new Color(1f, 0.6f, 0.6f);
    private static readonly Color WarningTint = new Color(1f, 0.9f, 0.5f);

    private SerializedObject _presetSerializedObject;

    private static Texture ErrorIcon => EditorGUIUtility.IconContent("console.erroricon.sml").image;
    private static Texture WarningIcon => EditorGUIUtility.IconContent("console.warnicon.sml").image;

    public override void OnInspectorGUI()
    {
        var manager = (CafeLayoutManager)target;

        serializedObject.Update();
        // Draw the manager's own field (the preset reference gets a custom dropdown below)
        DrawPropertiesExcluding(serializedObject, "m_Script", "activePreset");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Layout Preset", EditorStyles.boldLabel);
        DrawPresetDropdown();
        serializedObject.ApplyModifiedProperties();

        if (manager.activePreset == null)
        {
            EditorGUILayout.HelpBox(
                "Assign or select a Cafe Layout Preset to edit its fields here.", MessageType.Info);
            return;
        }

        List<LayoutIssue> issues = CafeLayoutValidator.Validate(manager.activePreset.ToLevelLayout());

        EditorGUILayout.Space();
        DrawSummary(issues);

        SerializedObject preset = GetPresetSerializedObject(manager.activePreset);
        preset.Update();

        DrawField(preset, "rows", issues, LayoutField.Dimensions);
        DrawField(preset, "cols", issues, LayoutField.Dimensions);
        DrawField(preset, "deliveryTileCoords", issues, LayoutField.DeliveryTileCoords);
        DrawField(preset, "floorPrefab", issues, LayoutField.FloorPrefab);
        DrawField(preset, "ceilingPrefab", issues, LayoutField.CeilingPrefab);
        DrawField(preset, "straightWallPrefab", issues, LayoutField.StraightWallPrefab);
        DrawField(preset, "cornerWallPrefab", issues, LayoutField.CornerWallPrefab);
        DrawField(preset, "deliveryTileData", issues, LayoutField.DeliveryTileData);
        DrawField(preset, "windowEndcapPrefab", issues, LayoutField.WindowEndcapPrefab);
        DrawField(preset, "elements", issues, LayoutField.Elements);

        preset.ApplyModifiedProperties();

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Save Preset to Disk"))
            {
                AssetDatabase.SaveAssetIfDirty(manager.activePreset);
            }
            if (GUILayout.Button("Ping Asset"))
            {
                EditorGUIUtility.PingObject(manager.activePreset);
            }
        }

        DrawReflectionProbeBake(manager);
    }

    // Performs a one-shot reflection probe capture once a layout is populated
    private static void DrawReflectionProbeBake(CafeLayoutManager manager)
    {
        bool layoutPopulated = manager.CafeRoot != null && manager.CafeRoot.transform.childCount > 0;
        if (!layoutPopulated)
        {
            return;
        }

        var bakers = Object.FindObjectsByType<CafeReflectionProbeBaker>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);

        if (bakers.Length == 0)
        {
            EditorGUILayout.HelpBox(
                "No CafeReflectionProbeBaker found in the loaded scenes. Open the Shift scene to bake the reflection probe.",
                MessageType.Info);
            return;
        }

        string label = bakers.Length > 1 ? $"Bake Reflection Probe ({bakers.Length})" : "Bake Reflection Probe";
        if (GUILayout.Button(label))
        {
            foreach (CafeReflectionProbeBaker baker in bakers)
            {
                baker.BakeImmediate();
            }
        }
    }

    private SerializedObject GetPresetSerializedObject(CafeLayoutPreset preset)
    {
        if (_presetSerializedObject == null || _presetSerializedObject.targetObject != preset)
        {
            _presetSerializedObject = new SerializedObject(preset);
        }
        return _presetSerializedObject;
    }

    // Draws a property, tinting its background and putting an icon badge (with the issue
    // messages in its tooltip) on the field label when it has issues
    private static void DrawField(SerializedObject preset, string propertyName,
        List<LayoutIssue> issues, LayoutField field)
    {
        SerializedProperty property = preset.FindProperty(propertyName);

        bool hasError = issues.Any(i => i.field == field && i.severity == LayoutIssueSeverity.Error);
        bool hasWarning = !hasError && issues.Any(i => i.field == field);

        string tooltip = (hasError || hasWarning)
            ? string.Join("\n", issues.Where(i => i.field == field).Select(i => i.message))
            : null;
        Texture icon = hasError ? ErrorIcon : hasWarning ? WarningIcon : null;

        Color previous = GUI.backgroundColor;
        if (hasError) GUI.backgroundColor = ErrorTint;
        else if (hasWarning) GUI.backgroundColor = WarningTint;

        // Most fields honor GUIContent.image on their own label, but a generic-struct foldout
        // (e.g. GridCoord) ignores it on the header — so for those we draw the struct's children
        // individually and put the badge on each child label instead (the same fields that get
        // the background tint)
        bool isStructFoldout = property.propertyType == SerializedPropertyType.Generic
            && !property.isArray && property.hasVisibleChildren;

        if (isStructFoldout)
        {
            property.isExpanded = EditorGUILayout.Foldout(
                property.isExpanded, new GUIContent(property.displayName, tooltip), true);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                SerializedProperty child = property.Copy();
                SerializedProperty end = property.GetEndProperty();
                bool enterChildren = true;
                while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
                {
                    enterChildren = false;
                    EditorGUILayout.PropertyField(
                        child, new GUIContent(child.displayName, icon, tooltip), true);
                }
                EditorGUI.indentLevel--;
            }
        }
        else
        {
            EditorGUILayout.PropertyField(
                property, new GUIContent(property.displayName, icon, tooltip), true);
        }

        GUI.backgroundColor = previous;
    }

    private static void DrawSummary(List<LayoutIssue> issues)
    {
        int errors = issues.Count(i => i.severity == LayoutIssueSeverity.Error);
        int warnings = issues.Count - errors;

        if (errors > 0)
        {
            string warnSuffix = warnings > 0 ? $", {warnings} warning(s)" : "";
            EditorGUILayout.HelpBox(
                $"{errors} error(s){warnSuffix} found. Preview was not updated. "
                + "Hover a field's badge for details.",
                MessageType.Error);
        }
        else if (warnings > 0)
        {
            EditorGUILayout.HelpBox(
                $"{warnings} warning(s) found. Preview was updated regardless. Hover a field's badge for details.",
                MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("Layout is valid.", MessageType.Info);
        }
    }

    private void DrawPresetDropdown()
    {
        var presets = AssetDatabase.FindAssets("t:CafeLayoutPreset")
            .Select(guid => AssetDatabase.LoadAssetAtPath<CafeLayoutPreset>(
                AssetDatabase.GUIDToAssetPath(guid)))
            .Where(p => p != null)
            .ToList();

        SerializedProperty presetProp = serializedObject.FindProperty("activePreset");
        var current = presetProp.objectReferenceValue as CafeLayoutPreset;

        var names = new[] { "(None)" }.Concat(presets.Select(p => p.name)).ToArray();
        int currentIndex = current != null ? presets.IndexOf(current) + 1 : 0;

        int selected = EditorGUILayout.Popup("Active Preset", currentIndex, names);
        if (selected != currentIndex)
        {
            // Assigning through the SerializedProperty triggers the manager's OnValidate,
            // which re-points the Changed subscription and rebuilds the preview
            presetProp.objectReferenceValue = selected == 0 ? null : presets[selected - 1];
        }
    }
}

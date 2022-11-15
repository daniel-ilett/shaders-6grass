using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GeometryGrassGUI : ShaderGUI
{
    public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
    {
        // Start a new section for grass albedo colors.
        GUILayout.Label("Grass Albedo Color", EditorStyles.boldLabel);

        Material target = editor.target as Material;

        MaterialProperty baseColor = FindProperty("_BaseColor", properties);
        GUIContent baseColorLabel = new GUIContent(baseColor.displayName, "Color of the grass blade at the ground level");
        editor.ShaderProperty(baseColor, baseColorLabel);

        MaterialProperty tipColor = FindProperty("_TipColor", properties);
        GUIContent tipColorLabel = new GUIContent(tipColor.displayName, "Color of the grass blade at the very tip");
        editor.ShaderProperty(tipColor, tipColorLabel);

        MaterialProperty baseTexture = FindProperty("_BaseTex", properties);
        GUIContent baseTextureLabel = new GUIContent(baseTexture.displayName, "Tint texture for each grass blade");
        editor.TexturePropertySingleLine(baseTextureLabel, baseTexture);

        GUILayout.Space(15);

        // Start a new section for grass size properties.
        GUILayout.Label("Grass Blade Size", EditorStyles.boldLabel);

        MaterialProperty bladeWidthMin = FindProperty("_BladeWidthMin", properties);
        GUIContent bladeWidthMinLabel = new GUIContent(bladeWidthMin.displayName, "Minimum width (in meters) of each grass blade.");
        editor.ShaderProperty(bladeWidthMin, bladeWidthMinLabel);

        MaterialProperty bladeWidthMax = FindProperty("_BladeWidthMax", properties);
        GUIContent bladeWidthMaxLabel = new GUIContent(bladeWidthMax.displayName, "Maximum width (in meters) of each grass blade.");
        editor.ShaderProperty(bladeWidthMax, bladeWidthMaxLabel);

        MaterialProperty bladeHeightMin = FindProperty("_BladeHeightMin", properties);
        GUIContent bladeHeightMinLabel = new GUIContent(bladeHeightMin.displayName, "Minimum height (in meters) of each grass blade.");
        editor.ShaderProperty(bladeHeightMin, bladeHeightMinLabel);

        MaterialProperty bladeHeightMax = FindProperty("_BladeHeightMax", properties);
        GUIContent bladeHeightMaxLabel = new GUIContent(bladeHeightMax.displayName, "Maximum height (in meters) of each grass blade.");
        editor.ShaderProperty(bladeHeightMax, bladeHeightMaxLabel);

        GUILayout.Space(15);

        // Start a new section for grass bend properties.
        GUILayout.Label("Grass Bend", EditorStyles.boldLabel);

        MaterialProperty bladeBendDistance = FindProperty("_BladeBendDistance", properties);
        GUIContent bladeBendDistanceLabel = new GUIContent(bladeBendDistance.displayName, "The maximum distance each blade can bend forward.");
        editor.ShaderProperty(bladeBendDistance, bladeBendDistanceLabel);

        MaterialProperty bladeBendCurve = FindProperty("_BladeBendCurve", properties);
        GUIContent bladeBendCurveLabel = new GUIContent(bladeBendCurve.displayName, "The amount of curvature of each blade of grass.");
        editor.ShaderProperty(bladeBendCurve, bladeBendCurveLabel);

        MaterialProperty bladeBendDelta = FindProperty("_BladeBendDelta", properties);
        GUIContent bladeBendDeltaLabel = new GUIContent(bladeBendDelta.displayName, "The amount of bend variation between blades.");
        editor.ShaderProperty(bladeBendDelta, bladeBendDeltaLabel);

        GUILayout.Space(15);

        // Start a new section for tessellation.
        GUILayout.Label("Tessellation", EditorStyles.boldLabel);

        MaterialProperty tessellationAmount = FindProperty("_TessAmount", properties);
        GUIContent tessellationAmountLabel = new GUIContent(tessellationAmount.displayName, "The higher this value, the closer the blades are.");
        editor.ShaderProperty(tessellationAmount, tessellationAmountLabel);

        MaterialProperty tessellationMinDistance = FindProperty("_TessMinDistance", properties);
        GUIContent tessellationMinDistanceLabel = new GUIContent(tessellationMinDistance.displayName, "Applies full tessellation when the camera is closer than this distance.");
        editor.ShaderProperty(tessellationMinDistance, tessellationMinDistanceLabel);

        MaterialProperty tessellationMaxDistance = FindProperty("_TessMaxDistance", properties);
        GUIContent tessellationMaxDistanceLabel = new GUIContent(tessellationMaxDistance.displayName, "Applies no extra tessellation when the camera is further than this distance.");
        editor.ShaderProperty(tessellationMaxDistance, tessellationMaxDistanceLabel);

        GUILayout.Space(15);

        // Start a new section for the grass visibility map.
        GUILayout.Label("Grass Visibility", EditorStyles.boldLabel);

        MaterialProperty grassMap = FindProperty("_GrassMap", properties);
        GUIContent grassMapLabel = new GUIContent(grassMap.displayName, "Visibility map. White = grass at full height, black = no grass");

        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(grassMapLabel, grassMap);
        if(EditorGUI.EndChangeCheck())
        {
            SetKeywordValue("VISIBILITY_ON", grassMap.textureValue, target);
        }

        if(grassMap.textureValue)
        {
            editor.TextureScaleOffsetProperty(grassMap);

            MaterialProperty grassThreshold = FindProperty("_GrassThreshold", properties);
            GUIContent grassThresholdLabel = new GUIContent(grassThreshold.displayName, "Grass is rendered when visibility map values exceed this threshold.");
            editor.ShaderProperty(grassThreshold, grassThresholdLabel);

            MaterialProperty grassFalloff = FindProperty("_GrassFalloff", properties);
            GUIContent grassFalloffLabel = new GUIContent(grassFalloff.displayName, "The amount of falloff between no grass and full grass.");
            editor.ShaderProperty(grassFalloff, grassFalloffLabel);
        }

        GUILayout.Space(15);

        // Start a new section for the wind map.
        GUILayout.Label("Wind", EditorStyles.boldLabel);

        MaterialProperty windMap = FindProperty("_WindMap", properties);
        GUIContent windMapLabel = new GUIContent(windMap.displayName, "Wind map. Each pixel is a vector controlling the wind direction.");

        EditorGUI.BeginChangeCheck();
        editor.TexturePropertySingleLine(windMapLabel, windMap);
        if(EditorGUI.EndChangeCheck())
        {
            SetKeywordValue("WIND_ON", windMap.textureValue, target);
        }

        if(windMap.textureValue)
        {
            editor.TextureScaleOffsetProperty(windMap);

            MaterialProperty windVelocity = FindProperty("_WindVelocity", properties);
            GUIContent windVelocityLabel = new GUIContent(windVelocity.displayName, "Strength and direction of the wind, expressed as a vector.");
            editor.ShaderProperty(windVelocity, windVelocityLabel);

            MaterialProperty windFrequency = FindProperty("_WindFrequency", properties);
            GUIContent windFrequencyLabel = new GUIContent(windFrequency.displayName, "How frequently the wind pulses over the grass.");
            editor.ShaderProperty(windFrequency, windFrequencyLabel);
        }
    }

    private void SetKeywordValue(string keyword, bool state, Material target)
    {
        if (state)
        {
            target.EnableKeyword(keyword);
        }
        else
        {
            target.DisableKeyword(keyword);
        }
    }
}

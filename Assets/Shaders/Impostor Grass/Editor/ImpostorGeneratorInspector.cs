using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ImpostorGenerator))]
public class ImpostorGeneratorInspector : Editor
{
    private ImpostorGenerator script;

    private void OnEnable()
    {
        script = target as ImpostorGenerator;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Bake Impostor", GUILayout.Height(40)))
        {
            script.CaptureImpostor();
        }
    }
}

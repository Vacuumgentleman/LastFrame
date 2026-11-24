using UnityEditor;
using UnityEngine;

namespace Senses2026BonusContent
{
    [CustomEditor(typeof(RagdolActivator))]
    public class RagdolActivatorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector first
            DrawDefaultInspector();
            // Get a reference to the actual target script
            RagdolActivator _ragdolActivator = (RagdolActivator)target;
            // Add a little space for clarity
            EditorGUILayout.Space(10);
            // Only allow activation in Play Mode
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            if (GUILayout.Button("Activate Ragdoll", GUILayout.Height(30)))
            {
                _ragdolActivator.ActivateRagdol();
            }

            EditorGUI.EndDisabledGroup(); // reset GUI state
        }
    }
}

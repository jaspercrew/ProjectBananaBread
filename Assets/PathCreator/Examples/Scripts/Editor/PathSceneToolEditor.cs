using UnityEditor;
using UnityEngine;

namespace PathCreator.Examples.Scripts.Editor
{
    [CustomEditor(typeof(PathSceneTool), true)]
    public class PathSceneToolEditor : UnityEditor.Editor
    {
        protected PathSceneTool PathTool;
        bool isSubscribed;

        public override void OnInspectorGUI()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                DrawDefaultInspector();

                if (check.changed)
                {
                    if (!isSubscribed)
                    {
                        TryFindPathCreator();
                        Subscribe();
                    }

                    if (PathTool.autoUpdate)
                    {
                        TriggerUpdate();

                    }
                }
            }

            if (GUILayout.Button("Manual Update"))
            {
                if (TryFindPathCreator())
                {
                    TriggerUpdate();
                    SceneView.RepaintAll();
                }
            }

        }


        void TriggerUpdate() {
            if (PathTool.pathCreator != null) {
                PathTool.TriggerUpdate();
            }
        }


        protected virtual void OnPathModified()
        {
            if (PathTool.autoUpdate)
            {
                TriggerUpdate();
            }
        }

        protected virtual void OnEnable()
        {
            PathTool = (PathSceneTool)target;
            PathTool.OnDestroyed += OnToolDestroyed;

            if (TryFindPathCreator())
            {
                Subscribe();
                TriggerUpdate();
            }
        }

        void OnToolDestroyed() {
            if (PathTool != null) {
                PathTool.pathCreator.pathUpdated -= OnPathModified;
            }
        }

 
        protected virtual void Subscribe()
        {
            if (PathTool.pathCreator != null)
            {
                isSubscribed = true;
                PathTool.pathCreator.pathUpdated -= OnPathModified;
                PathTool.pathCreator.pathUpdated += OnPathModified;
            }
        }

        bool TryFindPathCreator()
        {
            // Try find a path creator in the scene, if one is not already assigned
            if (PathTool.pathCreator == null)
            {
                if (PathTool.GetComponent<PathCreation.PathCreator>() != null)
                {
                    PathTool.pathCreator = PathTool.GetComponent<PathCreation.PathCreator>();
                }
                else if (FindObjectOfType<PathCreation.PathCreator>())
                {
                    PathTool.pathCreator = FindObjectOfType<PathCreation.PathCreator>();
                }
            }
            return PathTool.pathCreator != null;
        }
    }
}
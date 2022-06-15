using PathCreation;
using UnityEngine;

namespace PathCreator.Examples.Scripts
{
    [ExecuteInEditMode]
    public abstract class PathSceneTool : MonoBehaviour
    {
        public event System.Action OnDestroyed;
        public PathCreation.PathCreator pathCreator;
        public bool autoUpdate = true;

        protected VertexPath path {
            get {
                return pathCreator.path;
            }
        }

        public void TriggerUpdate() {
            PathUpdated();
        }


        protected virtual void OnDestroy() {
            if (OnDestroyed != null) {
                OnDestroyed();
            }
        }

        protected abstract void PathUpdated();
    }
}

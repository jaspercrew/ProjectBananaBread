using PathCreation;
using UnityEngine;

namespace PathCreator.Examples.Scripts {

    [ExecuteInEditMode]
    public class PathPlacer : PathSceneTool {

        public GameObject prefab;
        public GameObject holder;
        public float spacing = 3;

        const float MinSpacing = .1f;

        void Generate () {
            if (pathCreator != null && prefab != null && holder != null) {
                DestroyObjects ();

                VertexPath path2 = pathCreator.path;

                spacing = Mathf.Max(MinSpacing, spacing);
                float dst = 0;

                while (dst < path2.length) {
                    Vector3 point = path2.GetPointAtDistance (dst);
                    Quaternion rot = path2.GetRotationAtDistance (dst);
                    Instantiate (prefab, point, rot, holder.transform);
                    dst += spacing;
                }
            }
        }

        void DestroyObjects () {
            int numChildren = holder.transform.childCount;
            for (int i = numChildren - 1; i >= 0; i--) {
                DestroyImmediate (holder.transform.GetChild (i).gameObject, false);
            }
        }

        protected override void PathUpdated () {
            if (pathCreator != null) {
                Generate ();
            }
        }
    }
}
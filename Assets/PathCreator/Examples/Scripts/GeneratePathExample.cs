using PathCreation;
using UnityEngine;

namespace PathCreator.Examples.Scripts {
    // Example of creating a path at runtime from a set of points.

    [RequireComponent(typeof(PathCreation.PathCreator))]
    public class GeneratePathExample : MonoBehaviour {

        public bool closedLoop = true;
        public Transform[] waypoints;

        void Start () {
            if (waypoints.Length > 0) {
                // Create a new bezier path from the waypoints.
                BezierPath bezierPath = new BezierPath (waypoints, closedLoop, PathSpace.xyz);
                GetComponent<PathCreation.PathCreator> ().bezierPath = bezierPath;
            }
        }
    }
}
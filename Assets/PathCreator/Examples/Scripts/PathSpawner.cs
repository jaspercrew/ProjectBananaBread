using UnityEngine;

namespace PathCreator.Examples.Scripts {

    public class PathSpawner : MonoBehaviour {

        public PathCreation.PathCreator pathPrefab;
        public PathFollower followerPrefab;
        public Transform[] spawnPoints;

        void Start () {
            foreach (Transform t in spawnPoints) {
                var path = Instantiate (pathPrefab, t.position, t.rotation);
                var follower = Instantiate (followerPrefab);
                follower.pathCreator = path;
                path.transform.parent = t;
                
            }
        }
    }

}
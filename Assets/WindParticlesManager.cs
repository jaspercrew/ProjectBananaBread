using System;
using System.Collections.Generic;
// using UnityEditorInternal;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Random = UnityEngine.Random;

public class WindParticlesManager : MonoBehaviour
{
    public GameObject windParticlePrefab;
    
    private class WindParticle
    {
        public Transform Transform;
        public Vector3 ViewportPosition;
        public float OriginalY;
        // public float SpawnTime;
    }

    private LinkedList<WindParticle> windParticles = new LinkedList<WindParticle>();
    // private SpriteRenderer normalSquare;

    private const int FramesBetweenParticles = 120;
    private int framesSinceLastParticle = 0;
    
    // private readonly Vector3 particleViewportVelocity = new Vector3(0.001f, 0, 0);
    private const float BaseParticleXSpeed = 0.01f;
    
    private new Camera camera;
    // private float origCamY;
    private const float WindParticleZ = -3; // TODO

    private enum WindParticlesState
    {
        Turning, Stopping, Normal
    }

    private WindParticlesState state = WindParticlesState.Normal;

    // Start is called before the first frame update
    private void Start()
    {
        // normalSquare = GetComponent<SpriteRenderer>();
        // ComponentUtility.CopyComponent(GetComponent<SpriteRenderer>());
        camera = Camera.main;
        Debug.Assert(camera != null, "Camera.main is null!");
        // origCamY = camera.transform.position.y;
    }

    // Update is called once per frame
    private void Update()
    {
        // track camera position's x, but not y, and keep z constant
        // Vector3 camPos = camera.transform.position;
        // transform.position = new Vector3(camPos.x, origCamY, WindParticleZ);
        // transform.localPosition = Vector3.zero;
        
        framesSinceLastParticle++;
        if (framesSinceLastParticle >= FramesBetweenParticles)
            framesSinceLastParticle = FramesBetweenParticles;

        WindEmitter windZone = CharController.Instance.currentWindZone;
        // spawn new particle
        if (framesSinceLastParticle == FramesBetweenParticles && windZone != null)
        {
            framesSinceLastParticle = 0;
            SpawnParticle();
        }

        // go through existing particles, remove ones out of frame
        LinkedListNode<WindParticle> node = windParticles.First;

        float particleXSpeed = (WindEmitterChild.targetWind == null) ?
            0 : BaseParticleXSpeed * windZone.currentWind.speedOnPlayer;
        
        while (node != null)
        {
            LinkedListNode<WindParticle> next = node.Next;
            
            if (node.Value == null) // shouldn't happen
            {
                windParticles.Remove(node);
            }
            else if (!IsParticleOnScreen(node.Value))
            {
                // destroy and remove offscreen particles
                // print("wp died with lifetime " + (Time.time - node.Value.SpawnTime) + "s");
                Destroy(node.Value.Transform.gameObject);
                windParticles.Remove(node);
            }
            else
            {
                switch (state)
                {
                    case WindParticlesState.Turning:
                        break;
                    case WindParticlesState.Stopping:
                        break;
                    case WindParticlesState.Normal:
                        Vector3 pos = node.Value.Transform.position;
                        node.Value.Transform.position =
                            new Vector3(pos.x + particleXSpeed, node.Value.OriginalY, pos.z);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            node = next;
        }
    }

    // private bool IsTransformOnScreen(Transform t)
    // {
    //     Debug.Assert(camera != null, "camera != null");
    //  
    //     // check if all 4 corners are on screen
    //     Vector3 scale = t.localScale;
    //     Vector3 c = t.position;
    //     float halfW = scale.x / 2;
    //     float halfH = scale.y / 2;
    //     Vector3 topLeft = new Vector3(c.x - halfW, c.y + halfH, c.z);
    //     Vector3 bottomLeft = new Vector3(c.x - halfW, c.y - halfH, c.z);
    //     Vector3 topRight = new Vector3(c.x + halfW, c.y + halfH, c.z);
    //     Vector3 bottomRight = new Vector3(c.x + halfW, c.y - halfH, c.z);
    //
    //     return IsViewportPointOnScreen(camera.WorldToViewportPoint(topLeft)) &&
    //            IsViewportPointOnScreen(camera.WorldToViewportPoint(bottomLeft)) &&
    //            IsViewportPointOnScreen(camera.WorldToViewportPoint(topRight)) &&
    //            IsViewportPointOnScreen(camera.WorldToViewportPoint(bottomRight));
    // }

    private static bool IsParticleOnScreen(WindParticle wp)
    {
        return 0 <= wp.ViewportPosition.x && wp.ViewportPosition.x <= 1;
        // && 
        // 0 <= wp.viewportPosition.y && wp.viewportPosition.y <= 1;
    }

    private Vector3 ViewportToPosition(Vector3 viewportPoint)
    {
        Vector3 res = camera.ViewportToWorldPoint(viewportPoint);
        res.z = WindParticleZ;
        return res;
    }

    private void SpawnParticle()
    {
        WindEmitter wind = CharController.Instance.currentWindZone;
        if (wind == null)
            return;

        int side = (wind.currentWind.speedOnPlayer < 0) ? 1 : 0;
        Vector3 viewportPos = new Vector3(side, Random.value);
        Vector3 spawnPos = ViewportToPosition(viewportPos);
        spawnPos.z = WindParticleZ;
        
        GameObject newParticle = Instantiate(windParticlePrefab, transform);
        newParticle.transform.position = spawnPos;
        newParticle.transform.localScale = new Vector3(1, 0.1f, 1);

        WindParticle wp = new WindParticle
        {
            Transform = newParticle.transform,
            ViewportPosition = viewportPos,
            OriginalY = spawnPos.y,
            // SpawnTime = Time.time
        };
        windParticles.AddLast(wp);
        // print("adding new particle at vp=(" + viewportPos.x + "," + viewportPos.y + ") or wp=(" +
        //       spawnPos.x + "," + spawnPos.y + "," + spawnPos.z + ")");
    }
}

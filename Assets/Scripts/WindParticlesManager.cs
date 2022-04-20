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
        // public Vector3 ViewportPosition;
        // public float OriginalY;
        // public float SpawnTime;
    }

    private readonly LinkedList<WindParticle> windParticles = new LinkedList<WindParticle>();
    // private SpriteRenderer normalSquare;

    // private const int FramesBetweenParticles = 80; // TODO: dependent on speed?
    private const float TimeBetweenParticles = 0.2f;
    // private int framesSinceLastParticle = 0;
    private float timeSinceLastParticle;
    
    // private readonly Vector3 particleViewportVelocity = new Vector3(0.001f, 0, 0);
    private const float BaseParticleXSpeed = 0.1f;
    
    private new Camera camera;
    // private float origCamY;
    private const float WindParticleZ = -3; // TODO
    private const float SpeedToWidth = 0.3f;
    
    // TODO: 2d
    private bool isTurning = false;
    private float prevSpeed = 0;
    private float turnStartTime;
    private float turnDuration = CharController.ShiftCooldown / 3;
    private float turnStartSpeed;
    private float turnEndSpeed;

    // Start is called before the first frame update
    private void Start()
    {
        // Application.targetFrameRate = 30;
        // normalSquare = GetComponent<SpriteRenderer>();
        // ComponentUtility.CopyComponent(GetComponent<SpriteRenderer>());
        camera = Camera.main;
        Debug.Assert(camera != null, "Camera.main is null!");
        // origCamY = camera.transform.position.y;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        timeSinceLastParticle += Time.fixedDeltaTime;
        if (timeSinceLastParticle >= TimeBetweenParticles)
            timeSinceLastParticle = TimeBetweenParticles;

        WindEmitter windZone = CharController.Instance.currentWindZone;
        // spawn new particle
        if (Mathf.Approximately(timeSinceLastParticle, TimeBetweenParticles) && windZone != null)
        {
            timeSinceLastParticle = 0;
            SpawnParticle();
        }
        
        float newSpeed = (windZone == null)? 0 : windZone.currentWind.speedOnPlayer;

        // print("prev: " + prevSpeed + ", new: " + newSpeed);
        if (Math.Abs(prevSpeed - newSpeed) > float.Epsilon)
        {
            // print("turned!");
            isTurning = true;
            turnStartSpeed = prevSpeed;
            turnEndSpeed = newSpeed;
            turnStartTime = Time.time;
        }

        // go through existing particles, remove ones out of frame
        LinkedListNode<WindParticle> node = windParticles.First;
        
        while (node != null)
        {
            LinkedListNode<WindParticle> next = node.Next;
            
            if (node.Value == null) // shouldn't happen
            {
                windParticles.Remove(node);
            }
            else if (!IsParticleOnScreen(node.Value) || (!isTurning && newSpeed == 0))
            {
                // destroy and remove offscreen particles
                // print("wp died with lifetime " + (Time.time - node.Value.SpawnTime) + "s");
                Destroy(node.Value.Transform.gameObject);
                windParticles.Remove(node);
            }
            else
            {
                float windSpeed = (windZone == null) ? 0 : windZone.currentWind.speedOnPlayer;
                
                if (isTurning)
                {
                    float t = (Time.time - turnStartTime) / turnDuration;
                    // print("in process of turning: " + (t * 100) + "%");
                    if (t >= 1)
                        t = 1;

                    windSpeed = Mathf.Lerp(turnStartSpeed, turnEndSpeed, t);
                    
                    if (t >= 1)
                    {
                        isTurning = false;
                    }
                } 
                
                Vector3 pos = node.Value.Transform.position;
                Vector3 scale = node.Value.Transform.localScale;
                node.Value.Transform.position =
                    new Vector3(pos.x + BaseParticleXSpeed * windSpeed, pos.y, pos.z);
                node.Value.Transform.localScale =
                    new Vector3(SpeedToWidth * windSpeed, scale.y, scale.z);
            }

            node = next;
        }

        prevSpeed = newSpeed;
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

    private bool IsParticleOnScreen(WindParticle wp)
    {
        Vector2 vp = camera.WorldToViewportPoint(wp.Transform.position);
        return 0 <= vp.x && vp.x <= 1;
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
            // ViewportPosition = viewportPos,
            // OriginalY = spawnPos.y,
            // SpawnTime = Time.time
        };
        windParticles.AddLast(wp);
        // print("adding new particle at vp=(" + viewportPos.x + "," + viewportPos.y + ") or wp=(" +
        //       spawnPos.x + "," + spawnPos.y + "," + spawnPos.z + ")");
    }
}

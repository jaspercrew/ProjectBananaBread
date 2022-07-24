using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GravParticleManager : MonoBehaviour
{
    public GameObject gravParticlePrefab;
    
    private class WindParticle
    {
        public Transform Transform;
        // public Vector3 ViewportPosition;
        // public float OriginalY;
        // public float SpawnTime;
    }

    private FluidGravityZone fgz;

    private readonly LinkedList<WindParticle> windParticles = new LinkedList<WindParticle>();
    // private SpriteRenderer normalSquare;

    private const float TimeBetweenParticles = 0.2f;
    private float lastParticleSpawnTime = -1f;
    
    private const float BaseParticleSpeed = 0.1f;
    
    private const float GravParticleZ = -3; // TODO
    private const float SpeedToSize = 0.5f;
    private const float MinSize = 0.01f;
    
    private bool isTurning;
    private Vector2 prevSpeed;
    private float turnStartTime;
    private float turnDuration = 0.4f; // TODO
    private Vector2 turnStartSpeed;
    private Vector2 turnEndSpeed;

    // Start is called before the first frame update
    private void Start()
    {
        // camera = Camera.main;
        // Debug.Assert(camera != null, "Camera.main is null!");
        fgz = GetComponent<FluidGravityZone>();
        Debug.Assert(fgz != null, "fgz is null!");
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        // spawn new particle
        float timeNow = Time.time;
        if (timeNow - lastParticleSpawnTime >= TimeBetweenParticles)
        {
            SpawnParticle();
            lastParticleSpawnTime = timeNow;
        }
        
        Vector2 newSpeed = BaseParticleSpeed * (fgz.IsActive? fgz.activeGravityDirection : fgz.inactiveGravityDirection);

        //print("prev: " + prevSpeed + ", new: " + newSpeed);
        if ((prevSpeed - newSpeed).magnitude > float.Epsilon)
        {
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
                Debug.LogError("WIND NODE FAIL");
                windParticles.Remove(node);
            }
            else if (!IsParticleInZone(node.Value))
            {
                // TODO: dying
                // destroy and remove offscreen particles
                //print("wp died with lifetime " + (Time.time - node.Value.SpawnTime) + "s");
                //print(IsParticleOnScreen(node.Value));
                Destroy(node.Value.Transform.gameObject);
                windParticles.Remove(node);
            }
            else
            {
                Vector2 windSpeed = BaseParticleSpeed * (fgz.IsActive? fgz.activeGravityDirection : fgz.inactiveGravityDirection);
                
                if (isTurning)
                {
                    float t = (Time.time - turnStartTime) / turnDuration;
                    if (t >= 1)
                    {
                        t = 1;
                        isTurning = false;
                    }
                    // t = 1 - Mathf.Sqrt(1 - t * t);
                    t = (1 - Mathf.Cos(Mathf.PI * t)) / 2;
                    // print("in process of turning: " + (t * 100) + "%");

                    windSpeed = Vector2.Lerp(turnStartSpeed, turnEndSpeed, t);
                } 
                
                Vector3 pos = node.Value.Transform.position;
                Vector3 scale = node.Value.Transform.localScale;
                node.Value.Transform.position = new Vector3(
                    pos.x + windSpeed.x, 
                    pos.y + windSpeed.y, 
                      pos.z);
                node.Value.Transform.localScale = new Vector3(
                    // Mathf.Max(SpeedToSize * windSpeed.x, MinSize), 
                    // Mathf.Max(SpeedToSize * windSpeed.y, MinSize), 
                    SpeedToSize * Mathf.Max(windSpeed.x, 0.01f), // TODO ?????
                    SpeedToSize * Mathf.Max(windSpeed.y, 0.01f), 
                    scale.z);
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

    private bool IsParticleInZone(WindParticle wp)
    {
        Vector3 wpPos = wp.Transform.position;
        Transform fgzt = fgz.transform;
        Vector3 center = fgzt.position;
        Vector3 scale = fgzt.localScale;
        Rect r = new Rect(center.x - scale.x / 2 - 0.1f, center.y - scale.y / 2 - 0.1f, scale.x + 0.2f, scale.y + 0.2f);
        return r.Contains(wpPos);
        // float height = fgzt.localScale.y;
        // float width = fgzt.localScale.x;
        // return
        //     center.x - width / 2 <= wpPos.x && wpPos.x <= center.x + width / 2 &&
        //     center.y - height / 2 <= wpPos.y && wpPos.y <= center.y + height / 2;
    }

    // private Vector3 ViewportToPosition(Vector3 viewportPoint)
    // {
    //     Vector3 res = camera.ViewportToWorldPoint(viewportPoint);
    //     res.z = WindParticleZ;
    //     return res;
    // }

    private void SpawnParticle()
    {
        Transform fgzt = fgz.transform;
        Vector3 center = fgzt.position;
        Vector3 scale = fgzt.localScale;
        float height = scale.y;
        float width = scale.x;
        
        Vector2 grav = fgz.IsActive ? fgz.activeGravityDirection : fgz.inactiveGravityDirection;
        Vector3 spawnPos;
        if (grav.x == 0)
        {
            spawnPos = new Vector3(
                center.x + (Random.value - 0.5f) * width,
                center.y - Math.Sign(grav.y) * height / 2,
                GravParticleZ);
        } 
        else if (grav.y == 0) // this doesn't work
        {
            spawnPos = new Vector3(
                center.x - Math.Sign(grav.x) * width / 2,
                center.y + (Random.value - 0.5f) * height,
                GravParticleZ);
        }
        else
        {
            Debug.LogError("could not spawn wind particle, invalid grav direction!");
            return;
        }
        
        GameObject newParticle = Instantiate(gravParticlePrefab, transform);
        newParticle.transform.position = spawnPos;
        // newParticle.transform.localScale = new Vector3(1, 0.1f, 1);

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
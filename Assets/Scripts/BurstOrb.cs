using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstOrb : BeatEntity
{
    private BeatOrb[] circles;
    private int circleIterator = 0;
    public GameObject BeatCircle;
    public float radius = 1f;
    public float impulseForce = 1f;
    private CircleCollider2D circleCollider2D;
    private ParticleSystem particle;
    private bool applyForce = false;
    private bool playerInRange = false;
    protected override void Start()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
        particle = GetComponent<ParticleSystem>();
        int numCircles = actionMicroBeats.Count - 1;
        float angleGap = 2 * (float) Math.PI / numCircles;
        for (int i = 0; i < numCircles; i++)
        {
            Vector3 spawnPosition = new Vector3(radius * (float) Math.Cos(angleGap * i),
                                                radius * (float) Math.Sin(angleGap * i), 0);
            GameObject instantiatedCircle = Instantiate(BeatCircle, transform, true);
            instantiatedCircle.transform.position = transform.TransformPoint(spawnPosition);
        }
        circles = transform.GetComponentsInChildren<BeatOrb>();
        base.Start();
    }

    protected override void MicroBeatAction()
    {
        //print(circleIterator);
        
        if (circleIterator == actionMicroBeats.Count - 1)
        {
            PlayerImpulse();
            circleIterator = 0;
        }
        else
        {
            circles[circleIterator].Trigger();
            circleIterator++;
        }
    }

    private void PlayerImpulse()
    {
        particle.Emit(1);
        Trigger();
        if (((Vector2)CharController.Instance.transform.position - (Vector2)transform.position).magnitude < (transform.localScale.x * circleCollider2D.radius))
        {
            CharController.Instance.recentlyImpulsed = true;
            Vector2 direction = (CharController.Instance.transform.position - transform.position).normalized;
            CharController.Instance.GetComponent<Rigidbody2D>()
                .AddForce(direction * impulseForce, ForceMode2D.Impulse);
        }

    }
    


    protected override void MotifResetAction()
    {
        circleIterator = 0;
    }
    
    public Vector3 beatScale;
    public Vector3 restScale;
    public float timeToBeat;
    public float restSmoothTime;
    private IEnumerator MoveToScale(Vector3 _target)
    {
        Vector3 _curr = transform.localScale;
        Vector3 _initial = _curr;
        float _timer = 0;

        while (_curr != _target)
        {
            _curr = Vector3.Lerp(_initial, _target, _timer / timeToBeat);
            _timer += Time.deltaTime;

            transform.localScale = _curr;

            yield return null;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, restScale, restSmoothTime * Time.deltaTime);
    }

    public void Trigger()
    {
        StopCoroutine("MoveToScale");
        StartCoroutine("MoveToScale", beatScale);
    }
}

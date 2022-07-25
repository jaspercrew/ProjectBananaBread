using UnityEngine;

public class GravParticleManager : MonoBehaviour
{
    private FluidGravityZone fgz;
    private ParticleSystem ps;
    private ParticleSystem.MainModule psMain;
    private ParticleSystem.VelocityOverLifetimeModule psVelOverLife;

    private FluidGravityZone.GravityDirection prevGrav = FluidGravityZone.GravityDirection.None;

    private AnimationCurve halfToOneCurve;
    private ParticleSystem.MinMaxCurve speedNoneCurve;
    private ParticleSystem.MinMaxCurve speedUpCurve;
    private ParticleSystem.MinMaxCurve speedDownCurve;
    
    private void Start()
    {
        Transform parent = transform.parent;
        fgz = parent.GetComponent<FluidGravityZone>();
        ps = GetComponent<ParticleSystem>();
        psMain = ps.main;
        psVelOverLife = ps.velocityOverLifetime;
        ParticleSystem.ShapeModule sm = ps.shape;
        sm.scale = parent.localScale;
    
        halfToOneCurve = new AnimationCurve(new Keyframe(0, 0.5f), new Keyframe(1, 1));
        speedNoneCurve = new ParticleSystem.MinMaxCurve(0, halfToOneCurve);
        speedUpCurve = new ParticleSystem.MinMaxCurve(1, halfToOneCurve);
        speedDownCurve = new ParticleSystem.MinMaxCurve(-1, halfToOneCurve);
    }

    private void Update()
    {
        FluidGravityZone.GravityDirection newGrav =
            fgz.IsActive? fgz.activeGravityDirection : fgz.inactiveGravityDirection;
        
        if (newGrav == prevGrav) return;
        
        ps.Clear();
        
        switch (newGrav)
        {
            case FluidGravityZone.GravityDirection.North:
                psMain.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 0);
                psVelOverLife.x = speedNoneCurve;
                psVelOverLife.y = speedUpCurve;
                break;
            case FluidGravityZone.GravityDirection.South:
                psMain.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 180);
                psVelOverLife.x = speedNoneCurve;
                psVelOverLife.y = speedDownCurve;
                break;
            case FluidGravityZone.GravityDirection.East:
                psMain.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 90);
                psVelOverLife.x = speedUpCurve;
                psVelOverLife.y = speedNoneCurve;
                break;
            case FluidGravityZone.GravityDirection.West:
                psMain.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 270);
                psVelOverLife.x = speedDownCurve;
                psVelOverLife.y = speedNoneCurve;
                break;
            case FluidGravityZone.GravityDirection.None:  // should never happen but just in case
                Debug.LogWarning("gravity direction none! was this on purpose?");
                psMain.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 0);
                psVelOverLife.x = speedNoneCurve;
                psVelOverLife.y = speedNoneCurve;
                break;
            default:
                Debug.LogError("invalid gravity direction! (" + newGrav + ")");
                break;
        }

        prevGrav = newGrav;
    }
}
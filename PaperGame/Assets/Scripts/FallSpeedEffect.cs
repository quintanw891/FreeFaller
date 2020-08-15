using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallSpeedEffect : MonoBehaviour
{
    public Player player;
    public float baseParticleSpeed;
    public float maxAddedParticleSpeed;
    public float baseParticleAlpha;
    ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        float tiltUnitInterval = player.tiltAddedVerticalSpeed / player.maxAddedVerticalSpeed;

        var vel = ps.velocityOverLifetime;
        vel.speedModifier = baseParticleSpeed + tiltUnitInterval * maxAddedParticleSpeed;

        var col = ps.colorOverLifetime;
        Gradient grad = new Gradient();
        float alpha = baseParticleAlpha > tiltUnitInterval ? baseParticleAlpha : tiltUnitInterval;
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) });
        col.color = grad;
    }
}

using UnityEngine;

namespace UnderTheRain.World
{
    /// <summary>
    /// Procedural 2D drizzle particles for the sewers/surface neo-noir atmosphere.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class SewerRainAmbience : MonoBehaviour
    {
        [SerializeField] private float emissionRate = 65f;
        [SerializeField] private float fallSpeed = 14f;
        [SerializeField] private Vector2 areaSize = new Vector2(28f, 1f);
        [SerializeField] private Color dropletColor = new Color(0.55f, 0.72f, 0.82f, 0.32f);

        private void Awake()
        {
            ConfigureParticles();
        }

        private void ConfigureParticles()
        {
            var ps = GetComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 1.4f;
            main.startSpeed = fallSpeed;
            main.startSize = 0.08f;
            main.startColor = dropletColor;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 300;

            var emission = ps.emission;
            emission.rateOverTime = emissionRate;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(areaSize.x, areaSize.y, 1f);

            var velocity = ps.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = -1.2f;
            velocity.y = 0f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Stretch;
                renderer.lengthScale = 4.5f;
                renderer.velocityScale = 0.08f;

                var shader = Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    renderer.material = new Material(shader);
                }
            }
        }
    }
}

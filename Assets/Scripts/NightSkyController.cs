using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeFacility
{
    [DefaultExecutionOrder(-250)]
    public class NightSkyController : MonoBehaviour
    {
        [SerializeField] private Vector3 skyCenter = new Vector3(60f, 40f, 60f);
        [SerializeField] private float starRadius = 220f;
        [SerializeField] private int starCount = 450;
        [SerializeField] private float minStarSize = 0.12f;
        [SerializeField] private float maxStarSize = 0.32f;

        [SerializeField] private Color moonlightColor = new Color(0.72f, 0.8f, 1f, 1f);
        [SerializeField] private Color skyTint = new Color(0.06f, 0.1f, 0.22f, 1f);
        [SerializeField] private Color groundTint = new Color(0.01f, 0.01f, 0.02f, 1f);
        [SerializeField] private Color ambientSkyColor = new Color(0.09f, 0.12f, 0.2f, 1f);
        [SerializeField] private Color ambientEquatorColor = new Color(0.03f, 0.04f, 0.08f, 1f);
        [SerializeField] private Color ambientGroundColor = new Color(0.01f, 0.01f, 0.02f, 1f);
        [SerializeField] private Color fogColor = new Color(0.03f, 0.05f, 0.09f, 1f);

        private Material runtimeSkyboxMaterial;
        private Material runtimeStarMaterial;
        private ParticleSystem starParticleSystem;

        private void Awake()
        {
            ApplyNightSky();
        }

        private void OnDestroy()
        {
            if (runtimeSkyboxMaterial != null)
            {
                Destroy(runtimeSkyboxMaterial);
            }

            if (runtimeStarMaterial != null)
            {
                Destroy(runtimeStarMaterial);
            }
        }

        private void ApplyNightSky()
        {
            ConfigureMoonlight();
            ConfigureSkybox();
            ConfigureAmbientLighting();
            CreateStars();
        }

        private void ConfigureMoonlight()
        {
            Light moon = RenderSettings.sun;

            if (moon == null)
            {
                Light[] sceneLights = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (Light lightSource in sceneLights)
                {
                    if (lightSource.type == LightType.Directional)
                    {
                        moon = lightSource;
                        break;
                    }
                }
            }

            if (moon == null)
            {
                GameObject lightObject = new GameObject("Moon Light");
                moon = lightObject.AddComponent<Light>();
                moon.type = LightType.Directional;
            }

            moon.color = moonlightColor;
            moon.intensity = 0.38f;
            moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(18f, -28f, 0f);
            RenderSettings.sun = moon;
        }

        private void ConfigureSkybox()
        {
            Shader skyboxShader = Shader.Find("Skybox/Procedural");
            if (skyboxShader == null)
            {
                return;
            }

            runtimeSkyboxMaterial = new Material(skyboxShader)
            {
                name = "Night Sky Runtime"
            };

            runtimeSkyboxMaterial.SetColor("_SkyTint", skyTint);
            runtimeSkyboxMaterial.SetColor("_GroundColor", groundTint);
            runtimeSkyboxMaterial.SetFloat("_AtmosphereThickness", 0.55f);
            runtimeSkyboxMaterial.SetFloat("_Exposure", 0.55f);
            runtimeSkyboxMaterial.SetFloat("_SunSize", 0.03f);
            runtimeSkyboxMaterial.SetFloat("_SunSizeConvergence", 8f);
            runtimeSkyboxMaterial.SetFloat("_SunDisk", 2f);

            RenderSettings.skybox = runtimeSkyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        private void ConfigureAmbientLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = ambientSkyColor;
            RenderSettings.ambientEquatorColor = ambientEquatorColor;
            RenderSettings.ambientGroundColor = ambientGroundColor;
            RenderSettings.reflectionIntensity = 0.35f;

            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 10f;
            RenderSettings.fogEndDistance = 160f;
        }

        private void CreateStars()
        {
            Transform existingStars = transform.Find("Stars");
            if (existingStars != null)
            {
                starParticleSystem = existingStars.GetComponent<ParticleSystem>();
                if (starParticleSystem != null)
                {
                    return;
                }
            }

            GameObject starsObject = new GameObject("Stars");
            starsObject.transform.SetParent(transform, false);
            starsObject.transform.position = skyCenter;

            starParticleSystem = starsObject.AddComponent<ParticleSystem>();
            ParticleSystemRenderer starRenderer = starsObject.GetComponent<ParticleSystemRenderer>();

            var main = starParticleSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.maxParticles = starCount;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.startLifetime = 9999f;
            main.startSpeed = 0f;

            var emission = starParticleSystem.emission;
            emission.enabled = false;

            var shape = starParticleSystem.shape;
            shape.enabled = false;

            Shader starShader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                                ?? Shader.Find("Particles/Standard Unlit")
                                ?? Shader.Find("Sprites/Default");

            if (starShader != null)
            {
                runtimeStarMaterial = new Material(starShader)
                {
                    name = "Night Stars Runtime"
                };
                runtimeStarMaterial.SetColor("_BaseColor", Color.white);
                runtimeStarMaterial.SetColor("_Color", Color.white);
                starRenderer.sharedMaterial = runtimeStarMaterial;
            }

            starRenderer.shadowCastingMode = ShadowCastingMode.Off;
            starRenderer.receiveShadows = false;
            starRenderer.renderMode = ParticleSystemRenderMode.Billboard;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[starCount];
            for (int i = 0; i < starCount; i++)
            {
                Vector3 direction = Random.onUnitSphere;
                direction.y = Mathf.Abs(direction.y) * 0.9f + 0.1f;
                direction.Normalize();

                float distance = Random.Range(starRadius * 0.82f, starRadius);
                float brightness = Random.Range(0.55f, 1f);
                Color starColor = Color.Lerp(new Color(0.72f, 0.8f, 1f, 1f), Color.white, Random.value);
                starColor *= brightness;
                starColor.a = brightness;

                particles[i].position = direction * distance;
                particles[i].startSize = Random.Range(minStarSize, maxStarSize);
                particles[i].startColor = starColor;
                particles[i].startLifetime = 9999f;
                particles[i].remainingLifetime = 9999f;
            }

            starParticleSystem.SetParticles(particles, particles.Length);
            starParticleSystem.Play();
        }
    }
}

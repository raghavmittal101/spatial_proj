using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSim : MonoBehaviour
{
    [SerializeField] Material material;
    enum _Type { Steam, Extinguisher, Fallout, Volcano, Waterfall};
    [SerializeField] _Type _type;


    void Start()
    {
        if (_type == _Type.Steam)
            SimulateSteam();
        else if (_type == _Type.Extinguisher)
            SimulateExtinguisher();
        else if (_type == _Type.Fallout)
            SimulateParticleFall();
        else if (_type == _Type.Volcano)
            SimulateVolcanoErruption();
        else if (_type == _Type.Waterfall)
            SimulateWaterfall();
    }

    void SimulateExtinguisher()
    {
        var ps = GetComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.6f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.5f));
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 150f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.07f;
        shape.position = Vector3.zero;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 0.3f);
        sizeCurve.AddKey(0.2f, 0.6f);
        sizeCurve.AddKey(1.0f, 1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(Color.white, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.6f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.4f;
        noise.frequency = 0.8f;
        noise.scrollSpeed = 0.3f;

        // Set renderer settings manually in Editor if needed
        ps.GetComponent<Renderer>().material = material;
    }
    void SimulateSteam()
    {
        var ps = GetComponent<ParticleSystem>();

        // Main settings
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, 0.35f));
        main.gravityModifier = -0.05f; // slight upward pull
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission settings
        var emission = ps.emission;
        emission.rateOverTime = 80f;

        // Shape settings
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.05f;
        shape.position = Vector3.zero;

        // Velocity over lifetime
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);

        // Size over lifetime
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 0.4f);
        sizeCurve.AddKey(0.5f, 1.0f);
        sizeCurve.AddKey(1.0f, 1.2f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0.0f),
                new GradientColorKey(Color.white, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.4f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // Noise for softness and turbulence
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.2f;
        noise.frequency = 0.4f;
        noise.scrollSpeed = 0.15f;

        // Set renderer settings manually in Editor if needed
        ps.GetComponent<Renderer>().material = material;
    }
    void SimulateParticleFall()
    {
        var ps = GetComponent<ParticleSystem>();

        // Main
        var main = ps.main;
        main.duration = 10f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.1f, 0.1f, 0.1f, 0.8f)); // dark gray ash
        main.gravityModifier = 0.2f; // simulate falling
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission
        var emission = ps.emission;
        emission.rateOverTime = 300f;

        // Shape
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 10f;
        shape.position = new Vector3(0, 15f, 0); // spawn high
        shape.arc = 360f;

        // Velocity over lifetime
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.1f, -0.3f); // slow descent
        velocity.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);

        // Size over lifetime (no growth)
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0.0f, 0.8f);
        sizeCurve.AddKey(1.0f, 1.0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // Color over lifetime (fade out slightly)
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f), 0.0f),
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0.0f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // Noise for turbulence
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.2f;

        // Optional: rotation over lifetime (spinning ash)
        var rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-20f, 20f);

    }
    void SimulateVolcanoErruption()
    {
        var ps = GetComponent<ParticleSystem>();

        // === Main Settings ===
        var main = ps.main;
        main.duration = 4f;
        main.loop = false; // one-shot burst
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 20f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 2.5f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.2f, 0.2f, 0.2f, 0.9f), // dark gray ash
            new Color(0.1f, 0.1f, 0.1f, 1f));  // near black
        main.gravityModifier = -0.1f; // simulate rising ash
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1000;

        // === Emission ===
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 500, 700)
        });

        // === Shape ===
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 1f;
        shape.radiusMode = ParticleSystemShapeMultiModeValue.Random;
        shape.arc = 360f;

        // === Velocity over Lifetime ===
        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(10f, 15f);
        velocity.x = new ParticleSystem.MinMaxCurve(-4f, 4f);
        velocity.z = new ParticleSystem.MinMaxCurve(-4f, 4f);

        // === Size over Lifetime ===
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 0.6f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // === Color over Lifetime ===
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 0.0f),
                new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        // === Noise (for turbulent smoke) ===
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 1.5f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.3f;

        // === Rotation ===
        var rot = ps.rotationOverLifetime;
        rot.enabled = true;
        rot.z = new ParticleSystem.MinMaxCurve(-30f, 30f);
    }


    [SerializeField] Material waterMaterial;
    [SerializeField] Material splashMaterial;

    void SimulateWaterfall()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "SplashSurface";
        ground.transform.position = new Vector3(0, 0, 0);
        ground.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
        ground.GetComponent<Renderer>().material.color = Color.gray;
        ground.AddComponent<BoxCollider>();

        // === Waterfall Particle System ===
        GameObject waterfall = new GameObject("Waterfall");
        waterfall.transform.position = new Vector3(0, 5f, 0);
        var ps = waterfall.AddComponent<ParticleSystem>();
        var psRenderer = waterfall.GetComponent<ParticleSystemRenderer>();
        if (waterMaterial != null)
            psRenderer.material = waterMaterial;

        var main = ps.main;
        main.loop = true;
        main.startLifetime = 1.5f;
        main.startSpeed = 7f;
        main.startSize = 0.1f;
        main.startColor = new Color(0.5f, 0.7f, 1f, 0.7f); // light blue water
        main.gravityModifier = 1.2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 400f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(1f, 0.1f, 0.5f);

        // Collision with surface
        var collision = ps.collision;
        collision.enabled = true;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.dampen = 0.1f;
        collision.bounce = 0.3f;
        collision.lifetimeLoss = 0.4f;

        // Add slight noise for realism
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 0.5f;

        // === Splash Particles on Bounce ===
        var trails = ps.trails;
        trails.enabled = true;
        trails.mode = ParticleSystemTrailMode.PerParticle;
        trails.ribbonCount = 1;
        trails.lifetime = 0.3f;
        trails.dieWithParticles = true;
        trails.widthOverTrail = 0.2f;

        // Optionally add splash effect using Sub Emitters
        var subEmitters = ps.subEmitters;
        subEmitters.enabled = true;
        subEmitters.AddSubEmitter(CreateSplash(), ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritNothing);

        ps.Play();
    }


    ParticleSystem CreateSplash()
    {
        GameObject splash = new GameObject("SplashEmitter");
        var ps = splash.AddComponent<ParticleSystem>();
        var psRenderer = splash.GetComponent<ParticleSystemRenderer>();
        if (splashMaterial != null)
            psRenderer.material = splashMaterial;

        var main = ps.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 1.5f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.5f, 0.7f, 1f, 0.5f);
        main.gravityModifier = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.burstCount = 1;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 10)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.1f;

        return ps;
    }



}



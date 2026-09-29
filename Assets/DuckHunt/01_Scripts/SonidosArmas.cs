using UnityEngine;

/// <summary>
/// Generador procedural de efectos de sonido para armas, explosiones, fanfarria de victoria y power-ups.
/// </summary>
public static class SonidosArmas
{
    private static AudioClip _sonidoPistola;
    private static AudioClip _sonidoEscopeta;
    private static AudioClip _sonidoRifle;
    private static AudioClip _sonidoLanzaCohetes;
    private static AudioClip _sonidoExplosion;
    private static AudioClip _sonidoVictoria;
    private static AudioClip _sonidoExtraLife;
    private static AudioClip _sonidoShield;
    private static AudioClip _sonidoDoublePoints;
    private static AudioClip _sonidoShieldAbsorb;
    private static AudioClip _sonidoPajaroAleteo;

    public static AudioClip SonidoPistola
    {
        get
        {
            if (_sonidoPistola == null) _sonidoPistola = GenerarSonidoPistola();
            return _sonidoPistola;
        }
    }

    public static AudioClip SonidoVictoria
    {
        get
        {
            if (_sonidoVictoria == null) _sonidoVictoria = GenerarSonidoVictoria();
            return _sonidoVictoria;
        }
    }

    public static AudioClip SonidoEscopeta
    {
        get
        {
            if (_sonidoEscopeta == null) _sonidoEscopeta = GenerarSonanteEscopeta();
            return _sonidoEscopeta;
        }
    }

    public static AudioClip SonidoRifle
    {
        get
        {
            if (_sonidoRifle == null) _sonidoRifle = GenerarSonidoRifle();
            return _sonidoRifle;
        }
    }

    public static AudioClip SonidoLanzaCohetes
    {
        get
        {
            if (_sonidoLanzaCohetes == null) _sonidoLanzaCohetes = GenerarSonidoLanzaCohetes();
            return _sonidoLanzaCohetes;
        }
    }

    public static AudioClip SonidoExplosion
    {
        get
        {
            if (_sonidoExplosion == null) _sonidoExplosion = GenerarSonidoExplosion();
            return _sonidoExplosion;
        }
    }

    public static AudioClip SonidoPowerUpExtraLife
    {
        get
        {
            if (_sonidoExtraLife == null) _sonidoExtraLife = GenerarSonidoExtraLife();
            return _sonidoExtraLife;
        }
    }

    public static AudioClip SonidoPowerUpShield
    {
        get
        {
            if (_sonidoShield == null) _sonidoShield = GenerarSonidoShield();
            return _sonidoShield;
        }
    }

    public static AudioClip SonidoPowerUpDoublePoints
    {
        get
        {
            if (_sonidoDoublePoints == null) _sonidoDoublePoints = GenerarSonidoDoublePoints();
            return _sonidoDoublePoints;
        }
    }

    public static AudioClip SonidoShieldAbsorb
    {
        get
        {
            if (_sonidoShieldAbsorb == null) _sonidoShieldAbsorb = GenerarSonidoShieldAbsorb();
            return _sonidoShieldAbsorb;
        }
    }

    public static AudioClip SonidoPajaroAleteo
    {
        get
        {
            if (_sonidoPajaroAleteo == null) _sonidoPajaroAleteo = GenerarSonidoAleteo();
            return _sonidoPajaroAleteo;
        }
    }

    private static AudioClip GenerarSonidoPistola()
    {
        int sampleRate = 44100;
        float duration = 0.25f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 28f);
            float noise = (Random.value * 2f - 1f) * 0.7f;
            float pop = Mathf.Sin(2f * Mathf.PI * (350f - 250f * (t / duration)) * t) * 0.3f;
            samples[i] = (noise + pop) * env;
        }

        AudioClip clip = AudioClip.Create("ProceduralPistolShot", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonanteEscopeta()
    {
        int sampleRate = 44100;
        float duration = 0.55f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 12f);
            float lowBoom = Mathf.Sin(2f * Mathf.PI * (160f - 110f * t) * t) * 0.55f;
            float punch = (Random.value * 2f - 1f) * 0.85f * Mathf.Exp(-t * 22f);
            samples[i] = (lowBoom + punch) * env;
        }

        AudioClip clip = AudioClip.Create("ProceduralShotgunShot", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoRifle()
    {
        int sampleRate = 44100;
        float duration = 0.4f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 16f);
            float snap = (Random.value * 2f - 1f) * 0.5f;
            float ring = Mathf.Sin(2f * Mathf.PI * 480f * t) * 0.4f * Mathf.Exp(-t * 8f);
            samples[i] = (snap + ring) * env;
        }

        AudioClip clip = AudioClip.Create("ProceduralRifleShot", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoLanzaCohetes()
    {
        int sampleRate = 44100;
        float duration = 0.7f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Sin(Mathf.Clamp01(t / 0.1f) * Mathf.PI * 0.5f) * Mathf.Exp(-t * 4f);
            float whoosh = (Random.value * 2f - 1f) * Mathf.Sin(2f * Mathf.PI * (120f + 180f * t) * t);
            float roar = Mathf.Sin(2f * Mathf.PI * 70f * t);
            samples[i] = (whoosh * 0.7f + roar * 0.3f) * env;
        }

        AudioClip clip = AudioClip.Create("ProceduralRocketShot", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoExplosion()
    {
        int sampleRate = 44100;
        float duration = 1.2f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 4.5f);
            float noise = (Random.value * 2f - 1f);
            float boom = Mathf.Sin(2f * Mathf.PI * 55f * Mathf.Exp(-t * 2f) * t);
            samples[i] = (noise * 0.55f + boom * 0.45f) * env;
        }

        AudioClip clip = AudioClip.Create("ProceduralExplosionShot", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoVictoria()
    {
        int sampleRate = 44100;
        float duration = 2.2f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f };
        float noteDuration = 0.5f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int noteIndex = Mathf.Min((int)(t / noteDuration), notes.Length - 1);
            float noteFreq = notes[noteIndex];
            float noteLocalT = t - (noteIndex * noteDuration);

            float env = Mathf.Exp(-noteLocalT * 3.5f) * Mathf.Min(1f, noteLocalT * 30f);
            float tone = Mathf.Sin(2f * Mathf.PI * noteFreq * t) * 0.6f
                       + Mathf.Sin(2f * Mathf.PI * noteFreq * 2f * t) * 0.25f
                       + Mathf.Sin(2f * Mathf.PI * noteFreq * 3f * t) * 0.15f;

            samples[i] = tone * env * 0.5f;
        }

        AudioClip clip = AudioClip.Create("ProceduralVictoryFanfare", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoExtraLife()
    {
        int sampleRate = 44100;
        float duration = 0.6f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // 3 notas mágicas ascendentes alegres (Fa5 -> La5 -> Do6)
        float[] freqs = new float[] { 698.46f, 880f, 1046.5f };
        float segDur = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int idx = Mathf.Min((int)(t / segDur), freqs.Length - 1);
            float freq = freqs[idx];
            float localT = t - (idx * segDur);

            float env = Mathf.Exp(-localT * 8f) * Mathf.Min(1f, localT * 40f);
            float s = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.7f
                    + Mathf.Sin(2f * Mathf.PI * freq * 2f * t) * 0.3f;
            samples[i] = s * env * 0.6f;
        }

        AudioClip clip = AudioClip.Create("PowerUp_ExtraLife", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoShield()
    {
        int sampleRate = 44100;
        float duration = 0.8f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float freq = 300f + Mathf.Sin(t * 25f) * 80f + (t * 400f);
            float env = Mathf.Sin(Mathf.Clamp01(t / 0.1f) * Mathf.PI * 0.5f) * Mathf.Exp(-t * 2.5f);
            float hum = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f
                      + Mathf.Sin(2f * Mathf.PI * (freq * 0.5f) * t) * 0.3f;
            samples[i] = hum * env * 0.65f;
        }

        AudioClip clip = AudioClip.Create("PowerUp_Shield", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoDoublePoints()
    {
        int sampleRate = 44100;
        float duration = 0.7f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // Secuencia dorada brillante (Sol5 -> Si5 -> Re6 -> Sol6)
        float[] freqs = new float[] { 783.99f, 987.77f, 1174.66f, 1567.98f };
        float segDur = duration / freqs.Length;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            int idx = Mathf.Min((int)(t / segDur), freqs.Length - 1);
            float freq = freqs[idx];
            float localT = t - (idx * segDur);

            float env = Mathf.Exp(-localT * 9f) * Mathf.Min(1f, localT * 50f);
            float sparkle = Mathf.Sin(2f * Mathf.PI * freq * t) * 0.6f
                          + Mathf.Sin(2f * Mathf.PI * (freq * 1.5f) * t) * 0.25f
                          + Mathf.Sin(2f * Mathf.PI * (freq * 2.0f) * t) * 0.15f;
            samples[i] = sparkle * env * 0.6f;
        }

        AudioClip clip = AudioClip.Create("PowerUp_DoublePoints", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoShieldAbsorb()
    {
        int sampleRate = 44100;
        float duration = 0.35f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 14f);
            float buzz = Mathf.Sin(2f * Mathf.PI * 220f * t) * Mathf.Sin(2f * Mathf.PI * 60f * t) * 0.6f;
            float zap = (Random.value * 2f - 1f) * 0.4f;
            samples[i] = (buzz + zap) * env * 0.7f;
        }

        AudioClip clip = AudioClip.Create("Shield_Absorb", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private static AudioClip GenerarSonidoAleteo()
    {
        int sampleRate = 44100;
        float duration = 0.3f;
        int sampleCount = Mathf.FloorToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Sin(t / duration * Mathf.PI);
            float whoosh = (Random.value * 2f - 1f) * Mathf.Sin(2f * Mathf.PI * 90f * t);
            samples[i] = whoosh * env * 0.3f;
        }

        AudioClip clip = AudioClip.Create("Bird_Flap", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

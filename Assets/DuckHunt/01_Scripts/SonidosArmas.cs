using UnityEngine;

/// <summary>
/// Generador procedural de efectos de sonido para armas, explosiones y fanfarria de victoria.
/// </summary>
public static class SonidosArmas
{
    private static AudioClip _sonidoPistola;
    private static AudioClip _sonidoEscopeta;
    private static AudioClip _sonidoRifle;
    private static AudioClip _sonidoLanzaCohetes;
    private static AudioClip _sonidoExplosion;
    private static AudioClip _sonidoVictoria;

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
            if (_sonidoEscopeta == null) _sonidoEscopeta = GenerarSonidoEscopeta();
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

    private static AudioClip GenerarSonidoEscopeta()
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

        // Secuencia armónica de fanfarria / victoria (Do - Mi - Sol - Do agudo)
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
}

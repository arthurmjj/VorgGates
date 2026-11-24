using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VorgGates.Presentation;

namespace VorgGates.EditorTools
{
    /// <summary>Synthesizes every sound of the game, so no external audio or license is required.</summary>
    internal static class VorgAudioBuilder
    {
        private const int Rate = 22050;
        private const string Root = "Assets/_Project/Audio";

        public static AudioLibrary Build()
        {
            EnsureFolder(Root);
            EnsureFolder($"{Root}/Sfx");
            EnsureFolder($"{Root}/Music");

            var effects = new[]
            {
                Effect("swing_sword", () => NoiseSweep(0.17f, 900f, 4200f, 0.9f, 1)),
                Effect("swing_axe", () => Mix(NoiseSweep(0.3f, 350f, 1700f, 1f, 2), Tone(0.22f, 130f, 70f, 0.45f))),
                Effect("cast_staff", () => Mix(Tone(0.26f, 420f, 1250f, 0.6f, 18f), NoiseSweep(0.2f, 2500f, 5200f, 0.16f, 3))),
                Effect("hit", () => Mix(NoiseSweep(0.09f, 3000f, 700f, 0.8f, 4), Tone(0.13f, 170f, 60f, 0.8f))),
                Effect("player_hurt", () => Mix(Tone(0.24f, 240f, 95f, 0.75f, 26f), NoiseSweep(0.12f, 1500f, 450f, 0.4f, 5))),
                Effect("dodge", () => NoiseSweep(0.22f, 2200f, 500f, 0.55f, 6)),
                Effect("enemy_death", () => Mix(Tone(0.36f, 360f, 55f, 0.65f, 12f), NoiseSweep(0.26f, 1200f, 220f, 0.4f, 7))),
                Effect("arrow", () => Mix(NoiseSweep(0.13f, 5200f, 1900f, 0.55f, 8), Tone(0.06f, 520f, 300f, 0.25f))),
                Effect("telegraph", () => Tone(0.3f, 190f, 620f, 0.5f, 22f)),
                Effect("door_close", () => Mix(Tone(0.34f, 96f, 44f, 1f), NoiseSweep(0.12f, 1200f, 300f, 0.55f, 9), Tone(0.42f, 640f, 610f, 0.12f))),
                Effect("door_open", () => Mix(Tone(0.44f, 120f, 240f, 0.45f, 9f), Tone(0.46f, 780f, 980f, 0.13f), NoiseSweep(0.4f, 500f, 1200f, 0.14f, 10))),
                Effect("portal", () => Mix(Arpeggio(1.25f, new[] { 196f, 293.7f, 392f, 587.3f, 784f }, 0.46f), NoiseSweep(1.1f, 400f, 3600f, 0.13f, 11))),
                Effect("pickup", () => Arpeggio(0.55f, new[] { 523.3f, 659.3f, 784f, 1046.5f }, 0.62f)),
                Effect("heal", () => Mix(Tone(0.42f, 392f, 523.3f, 0.3f), Tone(0.42f, 587.3f, 784f, 0.22f))),
                Effect("boss_spawn", () => Mix(Tone(0.95f, 62f, 96f, 1f, 7f), Tone(0.9f, 124f, 190f, 0.38f, 11f), NoiseSweep(0.85f, 200f, 720f, 0.3f, 12))),
                Effect("boss_slam", () => Mix(Tone(0.52f, 88f, 30f, 1f), NoiseSweep(0.3f, 1400f, 160f, 0.75f, 13))),
                Effect("boss_death", () => Mix(Tone(1.45f, 210f, 28f, 0.9f, 8f), NoiseSweep(1.3f, 1500f, 90f, 0.5f, 14), Tone(1.2f, 105f, 18f, 0.5f))),
                Effect("victory", () => Mix(Arpeggio(2.3f, new[] { 261.6f, 329.6f, 392f, 523.3f, 659.3f, 784f, 1046.5f }, 0.5f), Tone(2.2f, 130.8f, 130.8f, 0.3f))),
                Effect("ui_move", () => Tone(0.05f, 880f, 960f, 0.45f)),
                Effect("ui_click", () => Mix(Tone(0.1f, 520f, 780f, 0.55f), NoiseSweep(0.03f, 3000f, 1600f, 0.2f, 15)))
            };

            var music = new[]
            {
                Music("music_crypt", () => MusicLoop(24f, new[] { 55f, 110f, 130.81f, 164.81f }, 0.1f, 0f, 31)),
                Music("music_forge", () => MusicLoop(24f, new[] { 41.2f, 82.41f, 98f, 116.54f }, 0.13f, 1f, 32)),
                Music("music_sanctuary", () => MusicLoop(24f, new[] { 36.71f, 73.42f, 87.31f, 103.83f, 155.56f }, 0.09f, 0.35f, 33))
            };

            var path = $"{Root}/AudioLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(path);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, path);
            }

            Kit.Wire(library, ("effects", effects.Cast<UnityEngine.Object>().ToArray()), ("music", music.Cast<UnityEngine.Object>().ToArray()));
            EditorUtility.SetDirty(library);
            return library;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private static AudioClip Effect(string name, Func<float[]> render) => WriteClip($"{Root}/Sfx/{name}.wav", render, false);

        private static AudioClip Music(string name, Func<float[]> render) => WriteClip($"{Root}/Music/{name}.wav", render, true);

        private static AudioClip WriteClip(string path, Func<float[]> render, bool isMusic)
        {
            if (!File.Exists(path))
            {
                WriteWav(path, Normalize(render(), isMusic ? 0.8f : 0.92f));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            var settings = importer.defaultSampleSettings;
            settings.loadType = isMusic ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = isMusic ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
            settings.quality = 0.55f;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = true;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static float[] NoiseSweep(float seconds, float fromHz, float toHz, float gain, int seed)
        {
            var random = new System.Random(seed);
            var samples = new float[Mathf.CeilToInt(seconds * Rate)];
            var low = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = i / (float)samples.Length;
                var cutoff = Mathf.Lerp(fromHz, toHz, t);
                var alpha = 1f - Mathf.Exp(-2f * Mathf.PI * cutoff / Rate);
                low += alpha * ((float)random.NextDouble() * 2f - 1f - low);
                var envelope = Mathf.Sin(Mathf.PI * Mathf.Pow(t, 0.55f));
                samples[i] = low * envelope * envelope * gain * 2.2f;
            }

            return samples;
        }

        private static float[] Tone(float seconds, float fromHz, float toHz, float gain, float vibratoHz = 0f)
        {
            var samples = new float[Mathf.CeilToInt(seconds * Rate)];
            var phase = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = i / (float)samples.Length;
                var frequency = Mathf.Lerp(fromHz, toHz, t);
                if (vibratoHz > 0f) frequency *= 1f + 0.05f * Mathf.Sin(2f * Mathf.PI * vibratoHz * i / Rate);
                phase += 2f * Mathf.PI * frequency / Rate;
                var attack = Mathf.Clamp01(i / (0.004f * Rate));
                var wave = Mathf.Sin(phase) + 0.28f * Mathf.Sin(2f * phase) + 0.1f * Mathf.Sin(3f * phase);
                samples[i] = wave * attack * Mathf.Pow(1f - t, 1.7f) * gain;
            }

            return samples;
        }

        private static float[] Arpeggio(float seconds, float[] notes, float gain)
        {
            var samples = new float[Mathf.CeilToInt(seconds * Rate)];
            var spacing = seconds * 0.62f / notes.Length;
            for (var n = 0; n < notes.Length; n++)
            {
                var start = Mathf.FloorToInt(n * spacing * Rate);
                var note = Tone(seconds - n * spacing, notes[n], notes[n], gain);
                for (var i = 0; i < note.Length && start + i < samples.Length; i++) samples[start + i] += note[i];
            }

            return samples;
        }

        private static float[] MusicLoop(float seconds, float[] chord, float gain, float pulse, int seed)
        {
            var count = Mathf.RoundToInt(seconds * Rate);
            var samples = new float[count];
            var random = new System.Random(seed);
            for (var n = 0; n < chord.Length; n++)
            {
                // Whole cycles per loop keep the music seamless when it repeats.
                var cycles = Mathf.Round(chord[n] * seconds);
                var swell = 1 + random.Next(3);
                var offset = (float)random.NextDouble() * Mathf.PI * 2f;
                var level = n == 0 ? 1f : 0.55f;
                for (var i = 0; i < count; i++)
                {
                    var t = i / (float)count;
                    var phase = 2f * Mathf.PI * cycles * t;
                    var breathe = 0.65f + 0.35f * Mathf.Sin(2f * Mathf.PI * swell * t + offset);
                    samples[i] += (Mathf.Sin(phase) + 0.22f * Mathf.Sin(2f * phase)) * breathe * level * gain;
                }
            }

            if (pulse > 0f)
            {
                const int beats = 24;
                for (var i = 0; i < count; i++)
                {
                    var beat = i / (float)count * beats;
                    var local = beat - Mathf.Floor(beat);
                    var hit = Mathf.Exp(-local * 9f) * Mathf.Sin(2f * Mathf.PI * 52f * local * (seconds / beats));
                    samples[i] += hit * gain * pulse * 1.6f;
                }
            }

            return samples;
        }

        private static float[] Mix(params float[][] layers)
        {
            var mixed = new float[layers.Max(layer => layer.Length)];
            foreach (var layer in layers)
            {
                for (var i = 0; i < layer.Length; i++) mixed[i] += layer[i];
            }

            return mixed;
        }

        private static float[] Normalize(float[] samples, float peak)
        {
            var max = samples.Max(Mathf.Abs);
            if (max <= 0.0001f) return samples;
            var gain = peak / max;
            for (var i = 0; i < samples.Length; i++) samples[i] *= gain;
            return samples;
        }

        private static void WriteWav(string path, float[] samples)
        {
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            var dataSize = samples.Length * 2;
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(Rate);
            writer.Write(Rate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);
            foreach (var sample in samples) writer.Write((short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
        }
    }
}

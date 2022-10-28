﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace rer
{
    internal class NPCRandomiser
    {
        private static object g_lock = new object();
        private static VoiceInfo[] g_voiceInfo = new VoiceInfo[0];
        private static VoiceInfo[] g_available = new VoiceInfo[0];

        private readonly RandoLogger _logger;
        private readonly RandoConfig _config;
        private readonly string _originalDataPath;
        private readonly string _modPath;
        private readonly GameData _gameData;
        private readonly Map _map;
        private readonly Rng _random;
        private readonly List<VoiceInfo> _pool = new List<VoiceInfo>();
        private readonly HashSet<VoiceSample> _randomized = new HashSet<VoiceSample>();

        public NPCRandomiser(RandoLogger logger, RandoConfig config, string originalDataPath, string modPath, GameData gameData, Map map, Rng random)
        {
            _logger = logger;
            _config = config;
            _originalDataPath = originalDataPath;
            _modPath = modPath;
            _gameData = gameData;
            _map = map;
            _random = random;
            LoadVoiceInfo(originalDataPath);
        }

        public void Randomise()
        {
            var playerActor = _config.Player == 0 ? "leon" : "claire";

            _pool.AddRange(g_available.Shuffle(_random));

            _logger.WriteHeading("Randomizing Characters, Voices:");
            foreach (var rdt in _gameData.Rdts)
            {
                var room = _map.GetRoom(rdt.RdtId);
                if (room == null)
                    continue;

                var currentCharacters = rdt.Enemies.Where(x => IsNpc(x.Type)).Select(x => x.Type).ToArray();
                var currentActors = currentCharacters.Select(x => GetActor(x)).ToArray();

                var npcs = new int[0];
                if (room.SupportedNpcs != null && room.SupportedNpcs.Length != 0)
                {
                    npcs = room.SupportedNpcs.Shuffle(_random);
                    foreach (var enemy in rdt.Enemies)
                    {
                        // Marvin edge case
                        if (rdt.RdtId.Stage == 1 && rdt.RdtId.Room == 2)
                        {
                            if (_config.Player == 0)
                            {
                                if (enemy.Offset != 0x1E1C)
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (enemy.Offset != 0x1DF6)
                                {
                                    continue;
                                }
                            }
                        }
                        // Ben edge case
                        else if (rdt.RdtId.Stage == 2 && rdt.RdtId.Room == 1)
                        {
                            if (enemy.Type == EnemyType.BenBertolucci1)
                            {
                                continue;
                            }
                        }

                        if (IsNpc(enemy.Type))
                        {
                            var currentNpcIndex = Array.IndexOf(currentCharacters, enemy.Type);
                            var newNpcType = (EnemyType)npcs[currentNpcIndex % npcs.Length];
                            _logger.WriteLine($"{rdt.RdtId}:{enemy.Id} (0x{enemy.Offset:X}) [{enemy.Type}] becomes [{newNpcType}]");
                            enemy.Type = newNpcType;
                        }
                    }
                }

                foreach (var sound in rdt.Sounds)
                {
                    var voice = new VoiceSample(_config.Player, rdt.RdtId.Stage + 1, sound.Id);
                    var (actor, kind) = GetVoice(voice);
                    if (actor != null)
                    {
                        if (kind == "radio")
                        {
                            RandomizeVoice(voice, actor, actor, kind);
                        }
                        if ((actor == playerActor && kind != "npc") || kind == "pc" || npcs.Length == 0)
                        {
                            RandomizeVoice(voice, actor, actor, null);
                        }
                        else
                        {
                            var currentNpcIndex = Array.IndexOf(currentActors, actor);
                            if (currentNpcIndex != -1)
                            {
                                var newNpcType = (EnemyType)npcs[currentNpcIndex % npcs.Length];
                                var newActor = GetActor(newNpcType) ?? actor;
                                RandomizeVoice(voice, actor, newActor, null);
                            }
                        }
                    }
                }
            }
        }

        private void RandomizeVoice(VoiceSample voice, string actor, string newActor, string? kind)
        {
            if (_randomized.Contains(voice))
                return;

            var randomVoice = GetRandomVoice(newActor, kind);
            if (randomVoice != null)
            {
                SetVoice(voice, randomVoice.Value);
                _randomized.Add(voice);
                _logger.WriteLine($"    {voice} [{actor}] becomes {randomVoice.Value} [{newActor}]");
            }
        }

        private VoiceSample? GetRandomVoice(string actor, string? kind)
        {
            var index = _pool.FindIndex(x => x.Actor == actor && ((kind == null && x.Kind != "radio") || x.Kind == kind));
            if (index == -1)
            {
                var newItems = g_voiceInfo.Where(x => x.Actor == actor).Shuffle(_random).ToArray();
                if (newItems.Length == 0)
                    return null;

                _pool.AddRange(newItems);
                index = _pool.Count - 1;
            }

            var voiceInfo = _pool[index];
            _pool.RemoveAt(index);
            return voiceInfo.Sample;
        }

        private void SetVoice(VoiceSample dst, VoiceSample src)
        {
            var srcPath = GetVoicePath(_originalDataPath, src);
            var dstPath = GetVoicePath(_modPath, dst);
            Directory.CreateDirectory(Path.GetDirectoryName(dstPath)!);
            File.Copy(srcPath, dstPath, true);
        }

        private static string GetVoicePath(string basePath, VoiceSample sample)
        {
            return Path.Combine(basePath, "PL" + sample.Player, "Voice", "stage" + sample.Stage, $"v{sample.Id:000}.sap");
        }

        private (string?, string?) GetVoice(VoiceSample sample)
        {
            var voiceInfo = g_voiceInfo.FirstOrDefault(x => x.Sample == sample);
            return (voiceInfo?.Actor, voiceInfo?.Kind);
        }

        private void ConvertSapFiles(string path)
        {
            var wavFiles = Directory.GetFiles(path, "*.wav", SearchOption.AllDirectories);
            var wavLen = wavFiles.GroupBy(x => new FileInfo(x).Length).Where(x => x.Count() > 1).ToArray();

            // var sapFiles = Directory.GetFiles(path, "*.sap", SearchOption.AllDirectories);
            // foreach (var sapFile in sapFiles)
            // {
            //     var wavFile = Path.ChangeExtension(sapFile, ".wav");
            //     var bytes = File.ReadAllBytes(sapFile);
            //     File.WriteAllBytes(wavFile, bytes.Skip(8).ToArray());
            //     File.Delete(sapFile);
            // }
        }

        private static bool IsNpc(EnemyType type) => type >= EnemyType.ChiefIrons1;

        private static string? GetActor(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.AdaWong1:
                case EnemyType.AdaWong2:
                    return "ada";
                case EnemyType.ClaireRedfield:
                case EnemyType.ClaireRedfieldCowGirl:
                case EnemyType.ClaireRedfieldNoJacket:
                    return "claire";
                case EnemyType.LeonKennedyBandaged:
                case EnemyType.LeonKennedyBlackLeather:
                case EnemyType.LeonKennedyCapTankTop:
                case EnemyType.LeonKennedyRpd:
                    return "leon";
                case EnemyType.SherryWithClairesJacket:
                case EnemyType.SherryWithPendant:
                    return "sherry";
                case EnemyType.MarvinBranagh:
                    return "marvin";
                case EnemyType.AnnetteBirkin1:
                case EnemyType.AnnetteBirkin2:
                    return "annette";
                case EnemyType.ChiefIrons1:
                case EnemyType.ChiefIrons2:
                    return "irons";
                case EnemyType.BenBertolucci1:
                case EnemyType.BenBertolucci2:
                    return "ben";
                case EnemyType.RobertKendo:
                    return "kendo";
                default:
                    return null;
            }
        }

        private static void LoadVoiceInfo(string originalDataPath)
        {
            lock (g_lock)
            {
                if (g_voiceInfo.Length == 0 || g_available.Length == 0)
                {
                    g_voiceInfo = LoadVoiceInfoFromJson();
                    g_available = RemoveDuplicateVoices(g_voiceInfo, originalDataPath);
                }
            }
        }

        private static VoiceInfo[] LoadVoiceInfoFromJson()
        {
            var json = Resources.voice;
            var voiceList = JsonSerializer.Deserialize<Dictionary<string, string>>(json, new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var voiceInfos = new List<VoiceInfo>();
            foreach (var kvp in voiceList!)
            {
                var path = kvp.Key;
                var player = int.Parse(path.Substring(2, 1));
                var stage = int.Parse(path.Substring(15, 1));
                var id = int.Parse(path.Substring(18, 3));
                var sample = new VoiceSample(player, stage, id);
                var actorParts = kvp.Value.Split('_');
                voiceInfos.Add(new VoiceInfo(sample, actorParts[0], actorParts.Length >= 2 ? actorParts[1] : ""));
            }

            return voiceInfos.ToArray();
        }

        private static VoiceInfo[] RemoveDuplicateVoices(VoiceInfo[] voiceInfos, string originalDataPath)
        {
            var distinct = voiceInfos.ToList();
            foreach (var group in voiceInfos.GroupBy(x => GetVoiceSize(originalDataPath, x)))
            {
                if (group.Count() <= 1)
                    continue;

                foreach (var item in group.Skip(1))
                {
                    distinct.RemoveAll(x => x.Sample == item.Sample);
                }
            }
            return distinct.ToArray();
        }

        private static int GetVoiceSize(string basePath, VoiceInfo vi)
        {
            var path = GetVoicePath(basePath, vi.Sample);
            return (int)new FileInfo(path).Length;
        }
    }

    [DebuggerDisplay("[{Actor}] {Sample}")]
    internal class VoiceInfo
    {
        public VoiceSample Sample { get; set; }
        public string Actor { get; set; }
        public string Kind { get; set; }

        public VoiceInfo(VoiceSample sample, string actor, string kind)
        {
            Sample = sample;
            Actor = actor;
            Kind = kind;
        }
    }

    [DebuggerDisplay("Player = {Player} Stage = {Stage} Id = {Id}")]
    public struct VoiceSample : IEquatable<VoiceSample>
    {
        public VoiceSample(int player, int stage, int id)
        {
            Player = player;
            Stage = stage;
            Id = id;
        }

        public int Player { get; set; }
        public int Stage { get; set; }
        public int Id { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is VoiceSample sample && Equals(sample);
        }

        public bool Equals(VoiceSample other)
        {
            return Player == other.Player &&
                   Stage == other.Stage &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Player;
            hash = hash * 23 + Stage;
            hash = hash * 23 + Id;
            return hash;
        }

        public static bool operator ==(VoiceSample left, VoiceSample right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VoiceSample left, VoiceSample right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"PL{Player}/Voice/stage{Stage}/v{Id:000}.sap";
        }
    }
}

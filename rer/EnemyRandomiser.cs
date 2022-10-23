﻿using System;
using System.Collections.Generic;
using System.Linq;
using rer.Opcodes;

namespace rer
{
    internal class EnemyRandomiser
    {
        private readonly RandoLogger _logger;
        private readonly RandoConfig _config;
        private readonly GameData _gameData;
        private readonly Map _map;
        private readonly Rng _rng;

        public EnemyRandomiser(RandoLogger logger, RandoConfig config, GameData gameData, Map map, Rng random)
        {
            _logger = logger;
            _config = config;
            _gameData = gameData;
            _map = map;
            _rng = random;
        }

        public void Randomise()
        {
            _logger.WriteHeading("Randomizing enemies:");
            foreach (var rdt in _gameData.Rdts)
            {
                var enemies = rdt.Enemies.ToArray();
                var logEnemies = enemies.Select(GetEnemyLogText).ToArray();
                RandomiseRoom(rdt);
                rdt.Save();
                for (int i = 0; i < logEnemies.Length; i++)
                {
                    var enemy = enemies[i];
                    var oldLog = logEnemies[i];
                    var newLog = GetEnemyLogText(enemy);
                    if (oldLog != newLog)
                    {
                        _logger.WriteLine($"{rdt.RdtId}:{enemy.Id} (0x{enemy.Offset:X}) {oldLog} becomes {newLog}");
                    }
                }
            }
        }

        private Rng.Table<EnemyType> CreateEnemyProbabilityTable(HashSet<EnemyType> includeTypes, HashSet<EnemyType> excludeTypes)
        {
            var table = _rng.CreateProbabilityTable<EnemyType>();
            switch (_config.EnemyDifficulty)
            {
                case 0:
                    AddIfSupported(EnemyType.Crow, 10);
                    AddIfSupported(EnemyType.ZombieArms, 10);
                    AddIfSupported(EnemyType.Spider, 10);
                    AddIfSupported(EnemyType.GiantMoth, 10);
                    AddIfSupported(EnemyType.Ivy, 15);
                    AddIfSupported(EnemyType.IvyPurple, 5);
                    AddIfSupported(EnemyType.Tyrant1, 1);
                    AddIfSupported(EnemyType.ZombieRandom, 30);
                    AddIfSupported(EnemyType.Cerebrus, 5);
                    AddIfSupported(EnemyType.LickerRed, 2);
                    AddIfSupported(EnemyType.LickerGrey, 2);
                    break;
                case 1:
                    AddIfSupported(EnemyType.Crow, 5);
                    AddIfSupported(EnemyType.ZombieArms, 5);
                    AddIfSupported(EnemyType.Spider, 5);
                    AddIfSupported(EnemyType.GiantMoth, 5);
                    AddIfSupported(EnemyType.Ivy, 5);
                    AddIfSupported(EnemyType.IvyPurple, 5);
                    AddIfSupported(EnemyType.Tyrant1, 5);
                    AddIfSupported(EnemyType.ZombieRandom, 40);
                    AddIfSupported(EnemyType.Cerebrus, 10);
                    AddIfSupported(EnemyType.LickerRed, 10);
                    AddIfSupported(EnemyType.LickerGrey, 5);
                    break;
                case 2:
                    AddIfSupported(EnemyType.Spider, 5);
                    AddIfSupported(EnemyType.GiantMoth, 5);
                    AddIfSupported(EnemyType.Ivy, 5);
                    AddIfSupported(EnemyType.IvyPurple, 5);
                    AddIfSupported(EnemyType.Tyrant1, 5);
                    AddIfSupported(EnemyType.ZombieRandom, 25);
                    AddIfSupported(EnemyType.Cerebrus, 25);
                    AddIfSupported(EnemyType.LickerRed, 15);
                    AddIfSupported(EnemyType.LickerGrey, 10);
                    break;
                case 3:
                default:
                    AddIfSupported(EnemyType.ZombieRandom, 5);
                    AddIfSupported(EnemyType.Spider, 5);
                    AddIfSupported(EnemyType.GiantMoth, 5);
                    AddIfSupported(EnemyType.Ivy, 2);
                    AddIfSupported(EnemyType.IvyPurple, 3);
                    AddIfSupported(EnemyType.Tyrant1, 5);
                    AddIfSupported(EnemyType.Cerebrus, 50);
                    AddIfSupported(EnemyType.LickerRed, 5);
                    AddIfSupported(EnemyType.LickerGrey, 20);
                    break;
            }
            return table;

            void AddIfSupported(EnemyType type, int prob)
            {
                if (type == EnemyType.ZombieRandom)
                {
                    var zp = (double)prob / _zombieTypes.Length;
                    foreach (var zt in _zombieTypes)
                    {
                        if (excludeTypes?.Contains(zt) != true &&
                            includeTypes?.Contains(zt) != false)
                        {
                            table.Add(zt, zp);
                        }
                    }
                }
                else
                {
                    if (excludeTypes?.Contains(type) != true &&
                        includeTypes?.Contains(type) != false)
                    {
                        table.Add(type, prob);
                    }
                }
            }
        }

        private static string GetEnemyLogText(SceEmSetOpcode enemy)
        {
            return $"[{enemy.Type},{enemy.State},{enemy.Ai},{enemy.SoundBank},{enemy.Texture}]";
        }

        private void RandomiseRoom(Rdt rdt)
        {
            var enemySpec = _map.GetRoom(rdt.RdtId)?.Enemies;
            if (enemySpec == null)
                return;

            if (enemySpec.Nop != null)
            {
                foreach (var offset in enemySpec.Nop)
                {
                    rdt.Nop(offset);
                    _logger.WriteLine($"{rdt.RdtId} (0x{offset:X2}) opcode removed");
                }
            }

            var includeTypes = enemySpec.IncludeTypes == null ?
                null :
                enemySpec.IncludeTypes.Select(x => (EnemyType)x).ToHashSet();
            var excludeTypes = enemySpec.ExcludeTypes == null ?
                null :
                enemySpec.ExcludeTypes.Select(x => (EnemyType)x).ToHashSet();
            var probTable = CreateEnemyProbabilityTable(includeTypes, excludeTypes);

            /*
            if (rdt.ToString() == "101")
                return;
            if (rdt.ToString() == "202")
                return;
            if (rdt.ToString() == "212")
                return;

            if (rdt.ToString() == "100")
                rdt.Nop(4888);

            if (rdt.ToString() == "105")
            {
                var enemyType3 = GetRandomZombieType();
                foreach (var enemy in rdt.Enemies)
                {
                    enemy.Type = enemyType3;
                    enemy.SoundBank = GetZombieSoundBank(enemy.Type);
                }
                return;
            }

            if (rdt.Enemies.Any(x => x.State == 72))
            {
                var enemyType3 = _rng.NextOf(EnemyType.ZombieRandom, EnemyType.ZombieNaked);
                foreach (var enemy in rdt.Enemies)
                {
                    enemy.Type = enemyType3;
                    enemy.SoundBank = GetZombieSoundBank(enemy.Type);
                }
                return;
            }
            */

#if MULTIPLE_ENEMY_TYPES
            var ids = rdt.Enemies.Select(x => x.Id).Distinct().ToArray();
            var enemyTypesId = ids.Select(x => EnemyType.Cerebrus).ToArray();
            if (enemyTypesId.Length >= 2)
                enemyTypesId[0] = EnemyType.LickerRed;
            if (enemyTypesId.Length >= 3)
                enemyTypesId[1] = EnemyType.LickerRed;
#else
            var ids = rdt.Enemies.Select(x => x.Id).Distinct().ToArray();
            var randomEnemyType = probTable.Next();
            var enemyTypesId = ids.Select(x => randomEnemyType).ToArray();
#endif

            foreach (var enemy in rdt.Enemies)
            {
                if (!ShouldChangeEnemy(enemy))
                    continue;

                var index = Array.IndexOf(ids, enemy.Id);
                var enemyType = enemyTypesId[index];
                enemy.Type = enemyType;
                if (!enemySpec.KeepState)
                    enemy.State = 0;
                if (!enemySpec.KeepAi)
                    enemy.Ai = 0;
                enemy.Texture = 0;
                if (enemySpec.Y != null)
                    enemy.Y = enemySpec.Y.Value;
                switch (enemyType)
                {
                    case EnemyType.ZombieGuy1:
                    case EnemyType.ZombieGuy2:
                    case EnemyType.ZombieGuy3:
                    case EnemyType.ZombieGirl:
                    case EnemyType.ZombieCop:
                    case EnemyType.ZombieTestSubject:
                    case EnemyType.ZombieScientist:
                    case EnemyType.ZombieNaked:
                    case EnemyType.ZombieRandom:
                        if (!enemySpec.KeepState)
                            enemy.State = _rng.NextOf<byte>(0, 1, 2, 3, 4, 6);
                        enemy.SoundBank = GetZombieSoundBank(enemyType);
                        break;
                    case EnemyType.Cerebrus:
                        enemy.State = 0;
                        if (_config.EnemyDifficulty >= 3)
                        {
                            // %50 of running
                            enemy.State = _rng.NextOf<byte>(0, 2);
                        }
                        else if (_config.EnemyDifficulty >= 2)
                        {
                            // %25 of running
                            enemy.State = _rng.NextOf<byte>(0, 0, 0, 2);
                        }
                        enemy.SoundBank = 12;
                        break;
                    case EnemyType.ZombieArms:
                        enemy.State = 0;
                        enemy.SoundBank = 17;
                        break;
                    case EnemyType.Crow:
                        enemy.State = 0;
                        enemy.SoundBank = 13;
                        break;
                    case EnemyType.BabySpider:
                    case EnemyType.Spider:
                        enemy.State = 0;
                        enemy.SoundBank = 16;
                        break;
                    case EnemyType.LickerRed:
                    case EnemyType.LickerGrey:
                        enemy.State = 0;
                        enemy.SoundBank = 14;
                        break;
                    case EnemyType.Cockroach:
                        enemy.State = 0;
                        enemy.SoundBank = 15;
                        break;
                    case EnemyType.Ivy:
                    case EnemyType.IvyPurple:
                        enemy.State = 0;
                        enemy.SoundBank = 19;
                        break;
                    case EnemyType.GiantMoth:
                        enemy.State = 0;
                        enemy.SoundBank = 23;
                        break;
                    case EnemyType.Tyrant1:
                        enemy.State = 0;
                        enemy.SoundBank = 18;
                        break;
                }
            }
        }

        private byte GetZombieSoundBank(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.ZombieCop:
                case EnemyType.ZombieGuy1:
                case EnemyType.ZombieGuy2:
                case EnemyType.ZombieGuy3:
                case EnemyType.ZombieRandom:
                case EnemyType.ZombieScientist:
                case EnemyType.ZombieTestSubject:
                    return _rng.NextOf<byte>(1, 2);
                case EnemyType.ZombieGirl:
                    return 10;
                case EnemyType.ZombieNaked:
                    return 46;
                default:
                    return 0;
            }
        }

        private bool ShouldChangeEnemy(SceEmSetOpcode enemy)
        {
            if (IsNpc(enemy.Type))
                return false;

            if (enemy.Type == EnemyType.ZombieBrad)
                return false;

            if (enemy.Type == EnemyType.LickerRed ||
                enemy.Type == EnemyType.LickerGrey)
            {
                // if (enemy.State == 0)
                    return true;
                // return false;
            }

            if (IsZombie(enemy.Type))
            {
                // Dead zombie (e.g. first licker corridor)
                if (enemy.State == 7)
                    return false;

                return true;
            }

            if (enemy.Type == EnemyType.Cerebrus || enemy.Type == EnemyType.Ivy || enemy.Type == EnemyType.IvyPurple)
                return true;

            if (enemy.Type == EnemyType.Crow)
                return true;

            return false;
        }

        private EnemyType GetRandomZombieType()
        {
            return _rng.NextOf(
                EnemyType.ZombieCop,
                EnemyType.ZombieGuy1,
                EnemyType.ZombieGirl,
                EnemyType.ZombieTestSubject,
                EnemyType.ZombieScientist,
                EnemyType.ZombieNaked,
                EnemyType.ZombieGuy2,
                EnemyType.ZombieGuy3,
                EnemyType.ZombieRandom);
        }

        private static void PrintAllEnemies(GameData gameData)
        {
            foreach (var rdt in gameData.Rdts)
            {
                if (rdt.Enemies.Count() != 0)
                {
                    Console.WriteLine($"RDT: {rdt.RdtId}:");
                    foreach (var enemy in rdt.Enemies)
                    {
                        Console.WriteLine($"    {enemy.Id}: {enemy.Type}, {enemy.State}, {enemy.Ai}, {enemy.SoundBank}, {enemy.Texture}");
                    }
                }
            }
        }

        private static bool IsZombie(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.ZombieCop:
                case EnemyType.ZombieBrad:
                case EnemyType.ZombieGuy1:
                case EnemyType.ZombieGirl:
                case EnemyType.ZombieTestSubject:
                case EnemyType.ZombieScientist:
                case EnemyType.ZombieNaked:
                case EnemyType.ZombieGuy2:
                case EnemyType.ZombieGuy3:
                case EnemyType.ZombieRandom:
                    return true;
                default:
                    return false;
            }
        }

        private static bool IsNpc(EnemyType type) => type >= EnemyType.ChiefIrons1;

        private static readonly HashSet<EnemyType> _enemyTypeSet = new HashSet<EnemyType>()
        {
            EnemyType.ZombieCop,
            EnemyType.ZombieBrad,
            EnemyType.ZombieGuy1,
            EnemyType.ZombieGirl,
            EnemyType.ZombieTestSubject,
            EnemyType.ZombieScientist,
            EnemyType.ZombieNaked,
            EnemyType.ZombieGuy2,
            EnemyType.ZombieGuy3,
            EnemyType.ZombieRandom,
            EnemyType.Cerebrus,
            EnemyType.Crow,
            EnemyType.LickerRed,
            EnemyType.Alligator,
            EnemyType.LickerGrey,
            EnemyType.Spider,
            EnemyType.BabySpider,
            EnemyType.Cockroach,
            EnemyType.Tyrant1,
            EnemyType.Tyrant2,
            EnemyType.ZombieArms,
            EnemyType.Ivy,
            EnemyType.IvyPurple,
            EnemyType.GiantMoth,
            EnemyType.Maggots
        };

        private static readonly EnemyType[] _zombieTypes = new[]
        {
            EnemyType.ZombieCop,
            EnemyType.ZombieGuy1,
            EnemyType.ZombieGirl,
            EnemyType.ZombieTestSubject,
            EnemyType.ZombieScientist,
            EnemyType.ZombieNaked,
            EnemyType.ZombieGuy2,
            EnemyType.ZombieGuy3,
            EnemyType.ZombieRandom
        };
    }
}
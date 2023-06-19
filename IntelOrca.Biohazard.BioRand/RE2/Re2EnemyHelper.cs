﻿using System;
using System.Collections.Generic;
using System.Linq;
using IntelOrca.Biohazard.Script;
using IntelOrca.Biohazard.Script.Opcodes;

namespace IntelOrca.Biohazard.RE2
{
    internal class Re2EnemyHelper : IEnemyHelper
    {
        private static readonly byte[] _zombieTypes = new byte[]
        {
            Re2EnemyIds.ZombieCop,
            Re2EnemyIds.ZombieGuy1,
            Re2EnemyIds.ZombieGirl,
            Re2EnemyIds.ZombieTestSubject,
            Re2EnemyIds.ZombieScientist,
            Re2EnemyIds.ZombieNaked,
            Re2EnemyIds.ZombieGuy2,
            Re2EnemyIds.ZombieGuy3,
            Re2EnemyIds.ZombieRandom,
            Re2EnemyIds.ZombieBrad
        };

        public Re2EnemyHelper()
        {
        }

        public string GetEnemyName(byte type)
        {
            var name = new Bio2ConstantTable().GetEnemyName(type);
            return name
                .Remove(0, 6)
                .Replace("_", " ");
        }

        public bool SupportsEnemyType(RandoConfig config, Rdt rdt, string difficulty, bool hasEnemyPlacements, byte enemyType)
        {
            if (config.RandomEnemyPlacement && hasEnemyPlacements)
            {
                return true;
            }
            else
            {
                var exclude = new HashSet<byte>();
                ExcludeEnemies(config, rdt, difficulty, x => exclude.Add(x));
                return !exclude.Contains(enemyType);
            }
        }

        private void ExcludeEnemies(RandoConfig config, Rdt rdt, string difficulty, Action<byte> exclude)
        {
            var types = rdt.Enemies
                .Select(x => x.Type)
                .Where(IsEnemy)
                .ToArray();

            if (types.Length != 1)
            {
                exclude(Re2EnemyIds.Birkin1);
            }

            if (difficulty == "medium" && config.EnemyDifficulty < 2)
            {
                exclude(Re2EnemyIds.LickerRed);
                exclude(Re2EnemyIds.LickerGrey);
                exclude(Re2EnemyIds.ZombieDog);
                exclude(Re2EnemyIds.Tyrant1);
            }
            else if (difficulty == "hard" && config.EnemyDifficulty < 3)
            {
                exclude(Re2EnemyIds.LickerRed);
                exclude(Re2EnemyIds.LickerGrey);
                exclude(Re2EnemyIds.ZombieDog);
                exclude(Re2EnemyIds.Tyrant1);
            }
        }

        public void BeginRoom(Rdt rdt)
        {
            // Mute dead zombies or vines, this ensures our random enemy type
            // will be heard
            foreach (var enemy in rdt.Enemies)
            {
                if (enemy.Type == Re2EnemyIds.Vines || (IsZombie(enemy.Type) && enemy.State == 2))
                {
                    enemy.SoundBank = 0;
                }
            }
        }

        public bool ShouldChangeEnemy(RandoConfig config, SceEmSetOpcode enemy)
        {
            switch (enemy.Type)
            {
                case Re2EnemyIds.Crow:
                case Re2EnemyIds.Spider:
                case Re2EnemyIds.GiantMoth:
                case Re2EnemyIds.LickerRed:
                case Re2EnemyIds.LickerGrey:
                case Re2EnemyIds.ZombieDog:
                case Re2EnemyIds.Ivy:
                case Re2EnemyIds.IvyPurple:
                case Re2EnemyIds.ZombieBrad:
                    return true;
                case Re2EnemyIds.MarvinBranagh:
                    // Edge case: Marvin is only a zombie in scenario B
                    return config.Scenario == 1;
                default:
                    return IsZombie(enemy.Type);
            }
        }

        public void SetEnemy(RandoConfig config, Rng rng, SceEmSetOpcode enemy, MapRoomEnemies enemySpec, byte enemyType)
        {
            switch (enemyType)
            {
                case Re2EnemyIds.ZombieGuy1:
                case Re2EnemyIds.ZombieGuy2:
                case Re2EnemyIds.ZombieGuy3:
                case Re2EnemyIds.ZombieGirl:
                case Re2EnemyIds.ZombieCop:
                case Re2EnemyIds.ZombieTestSubject:
                case Re2EnemyIds.ZombieScientist:
                case Re2EnemyIds.ZombieNaked:
                case Re2EnemyIds.ZombieRandom:
                case Re2EnemyIds.ZombieBrad:
                    if (!enemySpec.KeepState)
                        enemy.State = rng.NextOf<byte>(0, 1, 2, 3, 4, 6);
                    enemy.SoundBank = GetZombieSoundBank(enemyType);
                    break;
                case Re2EnemyIds.ZombieDog:
                    enemy.State = 0;
                    if (config.EnemyDifficulty >= 3)
                    {
                        // %50 of running
                        enemy.State = rng.NextOf<byte>(0, 2);
                    }
                    else if (config.EnemyDifficulty >= 2)
                    {
                        // %25 of running
                        enemy.State = rng.NextOf<byte>(0, 0, 0, 2);
                    }
                    enemy.SoundBank = 12;
                    break;
                case Re2EnemyIds.ZombieArms:
                    enemy.State = 0;
                    enemy.SoundBank = 17;
                    break;
                case Re2EnemyIds.Crow:
                    enemy.State = 0;
                    enemy.SoundBank = 13;
                    break;
                case Re2EnemyIds.BabySpider:
                case Re2EnemyIds.Spider:
                    enemy.State = 0;
                    enemy.SoundBank = 16;
                    break;
                case Re2EnemyIds.LickerRed:
                case Re2EnemyIds.LickerGrey:
                    enemy.State = 0;
                    enemy.SoundBank = 14;
                    break;
                case Re2EnemyIds.Cockroach:
                    enemy.State = 0;
                    enemy.SoundBank = 15;
                    break;
                case Re2EnemyIds.Ivy:
                case Re2EnemyIds.IvyPurple:
                    enemy.State = 0;
                    enemy.SoundBank = 19;
                    break;
                case Re2EnemyIds.GiantMoth:
                    enemy.State = 0;
                    enemy.SoundBank = 23;
                    break;
                case Re2EnemyIds.Tyrant1:
                    enemy.State = 0;
                    enemy.SoundBank = 18;
                    break;
                case Re2EnemyIds.Birkin1:
                    enemy.State = 1;
                    enemy.SoundBank = 24;
                    break;
            }
        }

        private static byte GetZombieSoundBank(byte type)
        {
            switch (type)
            {
                case Re2EnemyIds.ZombieCop:
                case Re2EnemyIds.ZombieGuy1:
                case Re2EnemyIds.ZombieGuy2:
                case Re2EnemyIds.ZombieGuy3:
                case Re2EnemyIds.ZombieRandom:
                case Re2EnemyIds.ZombieScientist:
                case Re2EnemyIds.ZombieTestSubject:
                case Re2EnemyIds.ZombieBrad:
                    return 1;
                case Re2EnemyIds.ZombieGirl:
                    return 10;
                case Re2EnemyIds.ZombieNaked:
                    return 46;
                default:
                    return 0;
            }
        }

        private static bool IsZombie(byte type)
        {
            switch (type)
            {
                case Re2EnemyIds.ZombieCop:
                case Re2EnemyIds.ZombieBrad:
                case Re2EnemyIds.ZombieGuy1:
                case Re2EnemyIds.ZombieGirl:
                case Re2EnemyIds.ZombieTestSubject:
                case Re2EnemyIds.ZombieScientist:
                case Re2EnemyIds.ZombieNaked:
                case Re2EnemyIds.ZombieGuy2:
                case Re2EnemyIds.ZombieGuy3:
                case Re2EnemyIds.ZombieRandom:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsEnemy(byte type)
        {
            return type < Re2EnemyIds.ChiefIrons1;
        }

        public bool IsUniqueEnemyType(byte type)
        {
            switch (type)
            {
                case Re2EnemyIds.Alligator:
                case Re2EnemyIds.Tyrant1:
                case Re2EnemyIds.Tyrant2:
                case Re2EnemyIds.Birkin1:
                case Re2EnemyIds.Birkin2:
                case Re2EnemyIds.Birkin3:
                case Re2EnemyIds.Birkin4:
                case Re2EnemyIds.Birkin5:
                    return true;
                default:
                    return false;
            }
        }

        public int GetEnemyTypeLimit(RandoConfig config, byte type)
        {
            byte[] limit;
            switch (type)
            {
                case Re2EnemyIds.Birkin1:
                    limit = new byte[] { 1 };
                    break;
                case Re2EnemyIds.ZombieDog:
                case Re2EnemyIds.GiantMoth:
                case Re2EnemyIds.Ivy:
                case Re2EnemyIds.IvyPurple:
                    limit = new byte[] { 2, 4, 6, 8 };
                    break;
                case Re2EnemyIds.LickerRed:
                case Re2EnemyIds.LickerGrey:
                case Re2EnemyIds.Tyrant1:
                    limit = new byte[] { 2, 3, 4, 6 };
                    break;
                default:
                    limit = new byte[] { 16 };
                    break;
            }
            var index = Math.Min(limit.Length - 1, config.EnemyDifficulty);
            return limit[index];
        }

        public SelectableEnemy[] GetSelectableEnemies() => new[]
        {
            new SelectableEnemy("Arms", "LightGray", new[] { Re2EnemyIds.ZombieArms }),
            new SelectableEnemy("Crow", "Black", new[] { Re2EnemyIds.Crow }),
            new SelectableEnemy("Spider", "YellowGreen", new[] { Re2EnemyIds.Spider }),
            new SelectableEnemy("Zombie", "LightGray", _zombieTypes),
            new SelectableEnemy("Moth", "DarkOliveGreen", new[] { Re2EnemyIds.GiantMoth }),
            new SelectableEnemy("Ivy", "SpringGreen", new[] { Re2EnemyIds.Ivy }),
            new SelectableEnemy("Licker", "IndianRed", new[] { Re2EnemyIds.LickerRed, Re2EnemyIds.LickerGrey }),
            new SelectableEnemy("Zombie Dog", "Black", new[] { Re2EnemyIds.ZombieDog }),
            new SelectableEnemy("Tyrant", "DarkGray", new[] { Re2EnemyIds.Tyrant1 }),
            new SelectableEnemy("Birkin", "IndianRed", new[] { Re2EnemyIds.Birkin1 }),
        };
    }
}
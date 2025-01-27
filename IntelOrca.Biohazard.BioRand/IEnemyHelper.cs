﻿using IntelOrca.Biohazard.Script.Opcodes;

namespace IntelOrca.Biohazard.BioRand
{
    internal interface IEnemyHelper
    {
        string GetEnemyName(byte type);
        bool SupportsEnemyType(RandoConfig config, RandomizedRdt rdt, bool hasEnemyPlacements, byte enemyType);
        bool ShouldChangeEnemy(RandoConfig config, SceEmSetOpcode enemy);
        void BeginRoom(RandomizedRdt rdt);
        void SetEnemy(RandoConfig config, Rng rng, SceEmSetOpcode enemy, MapRoomEnemies enemySpec, byte enemyTypeRaw);
        bool IsEnemy(byte type);
        bool IsUniqueEnemyType(byte type);
        SelectableEnemy[] GetSelectableEnemies();
        int GetEnemyTypeLimit(RandoConfig config, int difficulty, byte type);
        byte[] GetRequiredEsps(byte enemyType);
        byte[] GetReservedEnemyIds();
        byte[] GetEnemyDependencies(byte enemyType);
    }
}

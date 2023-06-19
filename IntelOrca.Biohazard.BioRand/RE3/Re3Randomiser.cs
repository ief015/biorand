﻿using IntelOrca.Biohazard.RE1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelOrca.Biohazard.RE3
{
    public class Re3Randomiser : BaseRandomiser
    {
        private ReInstallConfig? _reInstallConfig;

        protected override BioVersion BiohazardVersion => BioVersion.Biohazard3;
        internal override IDoorHelper DoorHelper => new Re3DoorHelper();
        internal override IItemHelper ItemHelper => new Re3ItemHelper();
        internal override IEnemyHelper EnemyHelper => new Re3EnemyHelper();
        internal override INpcHelper NpcHelper => new Re3NpcHelper();
        internal override string BGMPath => "DATA_A/SOUND";

        public Re3Randomiser(IBgCreator? bgCreator)
            : base(bgCreator)
        {
        }

        public override string GetPlayerName(int player) => "Jill";

        internal FileRepository CreateRepository(string installPath, string? modPath = null)
        {
            var repo = new FileRepository(installPath, null);
            AddArchives(installPath, repo);
            return repo;
        }

        internal void AddArchives(string installPath, FileRepository fileRepo)
        {
            var dataPath = GetDataPath(installPath);
            foreach (var path in Directory.GetFiles(dataPath, "rofs*.dat"))
            {
                fileRepo.AddRE3Archive(path);
            }
        }

        public override bool ValidateGamePath(string path)
        {
            return File.Exists(Path.Combine(path, "rofs1.dat"));
        }

        protected override string GetDataPath(string installPath)
        {
            return installPath;
        }

        protected override RdtId[] GetRdtIds(string dataPath)
        {
            var repo = CreateRepository(dataPath);
            var files = repo.GetFiles(Path.Combine(dataPath, "DATA_J", "RDT"));
            var rdts = files
                .Where(x => x.EndsWith(".RDT", StringComparison.OrdinalIgnoreCase))
                .Select(x => RdtId.Parse(x.Substring(x.Length - 7, 3)))
                .Where(x => x.Stage <= 4)
                .ToArray();
            return rdts;
        }

        protected override string GetRdtPath(string dataPath, RdtId rdtId, int player)
        {
            return Path.Combine(dataPath, "DATA_J", "RDT", $"R{rdtId}.RDT");
        }

        internal void AddMusicSelection(BgmRandomiser bgmRandomizer, ReInstallConfig reConfig, double volume)
        {
            var dataPath = GetDataPath(reConfig.GetInstallPath(BiohazardVersion));
            var srcBgmDirectory = Path.Combine(dataPath, BGMPath);
            bgmRandomizer.AddToSelection(GetBgmJson(), srcBgmDirectory, ".WAV", volume);
        }

        public override void Generate(RandoConfig config, ReInstallConfig reConfig, IRandoProgress progress, FileRepository fileRepository)
        {
            _reInstallConfig = reConfig;

            if (config.RandomBgm)
            {
                if (MusicAlbumSelected(config, "RE1") && !reConfig.IsEnabled(BioVersion.Biohazard1))
                {
                    throw new BioRandUserException("RE1 installation must be enabled to use RE1 assets.");
                }
                if (MusicAlbumSelected(config, "RE2") && !reConfig.IsEnabled(BioVersion.Biohazard2))
                {
                    throw new BioRandUserException("RE2 installation must be enabled to use RE2 assets.");
                }
            }
            if (!reConfig.IsEnabled(BioVersion.Biohazard3))
            {
                throw new BioRandUserException("RE3 installation must be enabled to randomize RE3.");
            }

            GenerateRdts(config, progress, fileRepository);

            base.Generate(config, reConfig, progress, fileRepository);
        }

        protected override void GenerateBGM(RandoConfig config, ReInstallConfig reConfig, IRandoProgress progress, FileRepository fileRepository)
        {
            base.GenerateBGM(config, reConfig, progress, fileRepository);

            // Reset all tracks to have 0 loop start and end
            var groups = new[] { 192, 135, 135 };
            var startOffset = 0x518AE0;

            var trackIndex = 0;
            var bw = new BinaryWriter(ExePatch);
            foreach (var g in groups)
            {
                for (int i = 0; i < g; i += 3)
                {
                    var trackName = g_trackOrder[trackIndex++];
                    if (File.Exists(fileRepository.GetModPath($"DATA_A/SOUND/{trackName}.wav")))
                    {
                        bw.Write((uint)startOffset + 8);
                        bw.Write((uint)8);
                        bw.Write((uint)0);
                        bw.Write((uint)0);
                    }
                    startOffset += 16 * 3;
                }
            }
        }

        private string[] GetEnabledPartners(RandoConfig config)
        {
            var enabledPLDs = GetAllPLDs().Intersect(GetEnabledNPCs(config)).ToArray();
            return enabledPLDs;
        }

        private string[] GetAllPLDs()
        {
            var pldFiles0 = DataManager
                .GetDirectories(BiohazardVersion, $"pld0")
                .Select(x => Path.GetFileName(x))
                .ToArray();
            return pldFiles0.OrderBy(x => x).ToArray();
        }

        private string[] GetEnabledNPCs(RandoConfig config)
        {
            var result = new List<string>();
            var allNPCs = GetNPCs();
            var enabledNPCs = config.EnabledNPCs;
            for (int i = 0; i < allNPCs.Length; i++)
            {
                if (enabledNPCs.Length > i && enabledNPCs[i])
                {
                    result.Add(allNPCs[i]);
                }
            }
            return result.ToArray();
        }

        public override string[] GetPlayerCharacters(int index)
        {
            var result = new List<string>();
            var pldFiles = DataManager
                .GetDirectories(BiohazardVersion, $"pld{index}")
                .ToArray();
            foreach (var pldPath in pldFiles)
            {
                var actor = Path.GetFileName(pldPath);
                result.Add(actor.ToActorString());
            }
            return result.ToArray();
        }

        protected override string[] GetDefaultNPCs()
        {
            return new[] { "jill.re3", "brad", "mikhail", "nikolai", "dario", "murphy", "tyrell", "carlos",
                "marvin", "irons" };
        }

        internal override string[] ChangePlayerCharacters(RandoConfig config, RandoLogger logger, GameData gameData, FileRepository fileRepository)
        {
            var actor = "jill.re3";
            var partner = "carlos";
            if (config.ChangePlayer)
            {
                // Change main
                var pldIndex = (config.Player == 0 ? config.Player0 : config.Player1) - 1;
                var pldPath = DataManager.GetDirectories(BiohazardVersion, $"pld{config.Player}")
                    .Skip(pldIndex)
                    .FirstOrDefault();
                actor = Path.GetFileName(pldPath);
                SwapPlayerCharacter(config, logger, fileRepository, 0, "C_02.VB", actor);

                // Change partner
                var rng = new Rng(config.Seed + config.Player);
                var enabledPLDs = GetEnabledPartners(config);
                if (enabledPLDs.Length != 0)
                {
                    partner = rng.NextOf(enabledPLDs);
                    SwapPlayerCharacter(config, logger, fileRepository, 8, "C_08.VB", partner);
                }
            }
            return new[] { actor, partner };
        }

        private void SwapPlayerCharacter(RandoConfig config, RandoLogger logger, FileRepository fileRepository,
            int pldIndex, string soundFileName, string actor)
        {
            var originalPlayerActor = pldIndex == 0 ? "jill.re3" : "carlos";
            var srcPldDir = DataManager.GetPath(BiohazardVersion, $"pld0\\{actor}");

            logger.WriteHeading($"Randomizing Player PL{pldIndex:X2}:");
            logger.WriteLine($"{originalPlayerActor} becomes {actor}");

            var targetPldDir = fileRepository.GetModPath($"DATA/PLD");
            Directory.CreateDirectory(targetPldDir);
            var pldFiles = Directory.GetFiles(srcPldDir);
            foreach (var pldPath in pldFiles)
            {
                var pldFile = Path.GetFileName(pldPath);
                if (pldFile.Length > 4 &&
                    (pldFile.EndsWith("pld", StringComparison.OrdinalIgnoreCase) ||
                     pldFile.EndsWith("plw", StringComparison.OrdinalIgnoreCase)))
                {
                    pldFile = $"PL{pldIndex:X2}{pldFile.Substring(4)}";
                    File.Copy(pldPath, Path.Combine(targetPldDir, pldFile), true);
                }
            }

            // Replace other PLDs
            if (actor != originalPlayerActor)
            {
                if (pldIndex == 0)
                {
                    var numbers = new[] { 1, 2, 3, 4, 5, 6, 7 };
                    var src = Path.Combine(targetPldDir, $"pl{config.Player:X2}.pld");
                    foreach (var n in numbers)
                    {
                        var dst = Path.Combine(targetPldDir, $"pl{n:X2}.pld");
                        File.Copy(src, dst, true);
                    }
                }

                var faceIndex = pldIndex == 0 ? 0 : 1;
                ChangePlayerInventoryFace(config, fileRepository, faceIndex, actor);

                var allHurtFiles = DataManager.GetHurtFiles(actor)
                    .Where(x => x.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                var hurtFiles = new string[4];
                foreach (var hurtFile in allHurtFiles)
                {
                    if (int.TryParse(Path.GetFileNameWithoutExtension(hurtFile), out var i))
                    {
                        if (i < hurtFiles.Length)
                        {
                            hurtFiles[i] = hurtFile;
                        }
                    }
                }
                if (hurtFiles.All(x => x != null))
                {
                    var filename = $"DATA/SOUND/{soundFileName}";
                    var vbSourcePath = fileRepository.GetDataPath(filename);
                    var vbSourceFile = fileRepository.GetBytes(vbSourcePath);
                    var vbTargetPath = fileRepository.GetModPath(filename);
                    Directory.CreateDirectory(Path.GetDirectoryName(vbTargetPath));

                    var vb = new VabFile(vbSourceFile);
                    var hurtIndex = new[] { 0, 1, 3, 2, 0 };
                    var sampleRates = new[] { 16000, 16000, 8000, 12000, 16000 };
                    for (int i = 0; i < hurtIndex.Length; i++)
                    {
                        var waveBuilder = new WaveformBuilder(channels: 1, sampleRates[i]);
                        waveBuilder.Append(hurtFiles[hurtIndex[i]]);
                        var pcmData = waveBuilder.GetPCM();
                        vb.SetSampleFromPCM(7 + i, pcmData);
                    }
                    vb.Write(vbTargetPath);
                }
            }
        }

        private void ChangePlayerInventoryFace(RandoConfig config, FileRepository fileRepository, int faceIndex, string actor)
        {
            if (BgCreator == null)
                return;

            var facePath = DataManager.GetPath(BiohazardVersion, Path.Combine($"pld{config.Player}", actor, "face.png"));
            if (!File.Exists(facePath))
                return;

            var filename = Path.Combine("DATA_J", "ETC2", "STMAIN0J.TIM");
            var inputTimPath = fileRepository.GetDataPath(filename);
            var outputTimPath = fileRepository.GetModPath(filename);
            if (File.Exists(outputTimPath))
                inputTimPath = outputTimPath;
            Directory.CreateDirectory(Path.GetDirectoryName(outputTimPath!));

            TimFile tim;
            using (var inputStream = fileRepository.GetStream(inputTimPath))
            {
                tim = new TimFile(inputStream);
            }
            BgCreator.DrawImage(tim, facePath, faceIndex * 40, 192, faceIndex == 0 ? 2 : 3);
            tim.Save(outputTimPath);
        }

        internal override void RandomizeNPCs(RandoConfig config, NPCRandomiser npcRandomiser)
        {
            if (_reInstallConfig!.IsEnabled(BioVersion.Biohazard1))
            {
                var dataPath = Re1Randomiser.FindDataPath(_reInstallConfig.GetInstallPath(BioVersion.Biohazard1));
                npcRandomiser.AddToSelection(BioVersion.Biohazard1, new FileRepository(dataPath));
            }
            if (_reInstallConfig!.IsEnabled(BioVersion.Biohazard2))
            {
                var dataPath = GetDataPath(_reInstallConfig.GetInstallPath(BioVersion.Biohazard2));
                // HACK should be helper function from RE 2 randomizer
                if (Directory.Exists(Path.Combine(dataPath, "data", "pl0", "rdt")))
                {
                    dataPath = Path.Combine(dataPath, "data");
                }
                npcRandomiser.AddToSelection(BioVersion.Biohazard2, new FileRepository(dataPath));
            }
            if (_reInstallConfig!.IsEnabled(BioVersion.Biohazard3))
            {
                var dataPath = GetDataPath(_reInstallConfig.GetInstallPath(BioVersion.Biohazard3));
                var fileRepository = new FileRepository(dataPath);
                var re3randomizer = new Re3Randomiser(null);
                re3randomizer.AddArchives(dataPath, fileRepository);
                npcRandomiser.AddToSelection(BioVersion.Biohazard3, fileRepository);
            }

            var emdFolders = DataManager.GetDirectories(BiohazardVersion, $"emd");
            foreach (var emdFolder in emdFolders)
            {
                var actor = Path.GetFileName(emdFolder);
                var files = Directory.GetFiles(emdFolder);
                foreach (var file in files)
                {
                    if (file.EndsWith(".emd", StringComparison.OrdinalIgnoreCase))
                    {
                        var hex = Path.GetFileName(file).Substring(2, 2);
                        if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out var result))
                        {
                            npcRandomiser.AddNPC((byte)result, file, actor);
                        }
                    }
                }
            }
        }

        protected override void SerialiseInventory(FileRepository fileRepository)
        {
            const uint AddressInventoryStart = 0x0052D20C;
            const uint AddressInventoryHeavyLength = 0x00460BFD;
            const uint AddressInventoryLightLength = 0x00460C00;
            // const uint AddressRefHard = 0x004609F3;
            const uint AddressRefEasy = 0x004609EB;

            if (Inventories.Count == 0)
                return;

            var inventoryX = Inventories[0];
            if (inventoryX == null)
                return;

            var jillInventory = inventoryX.Entries
                .Take(8)
                .Where(x => x.Type != 0)
                .ToArray();
            var carlosInventory = inventoryX.Entries
                .Skip(8)
                .Take(8)
                .Where(x => x.Type != 0)
                .ToArray();
            var inventories = new[] { jillInventory, carlosInventory };

            var bw = new BinaryWriter(ExePatch);

            // Hard then easy
            var offset = AddressInventoryStart;
            bw.Write(offset);
            var lengthPosition = bw.BaseStream.Position;
            bw.Write(0);

            var baseAddress = bw.BaseStream.Position;
            uint easyAddress = 0;
            for (int j = 0; j < 2; j++)
            {
                if (j == 1)
                    easyAddress = (uint)(offset + bw.BaseStream.Position - baseAddress);

                // Jill then Carlos
                for (int i = 0; i < 2; i++)
                {
                    var inventory = inventories[i];
                    foreach (var entry in inventory)
                    {
                        if (entry.Type == 0)
                            break;

                        bw.Write((byte)entry.Type);
                        bw.Write((byte)entry.Count);
                        bw.Write((byte)GetInventoryQuantityStyle(entry.Type));
                        bw.Write((byte)0);
                    }
                    bw.Write((int)-1);
                }
                bw.Write((int)0);
            }

            // Fix length
            var backupPosition = bw.BaseStream.Position;
            bw.BaseStream.Position = lengthPosition;
            bw.Write((uint)(backupPosition - baseAddress));
            bw.BaseStream.Position = backupPosition;

            // Write heavy Jill num items
            bw.Write(AddressInventoryHeavyLength);
            bw.Write(1);
            bw.Write((byte)((jillInventory.Length + 1) * 4));

            // Write easy address location
            bw.Write(AddressRefEasy);
            bw.Write(4);
            bw.Write(easyAddress);

            // Write light Jill num items
            bw.Write(AddressInventoryLightLength);
            bw.Write(1);
            bw.Write((byte)((jillInventory.Length + 1) * 4));
        }

        private byte GetInventoryQuantityStyle(byte type)
        {
            const byte None = 0b00;
            const byte Digit = 0b01;
            const byte Percent = 0b10;
            const byte Infinity = 0b11;
            const byte Green = 0 << 2;
            const byte Red = 1 << 2;
            const byte Orange = 2 << 2;
            const byte Blue = 3 << 2;
            const byte Automatic = 0b10000;

            switch (type)
            {
                case Re3ItemIds.HandgunSigpro:
                case Re3ItemIds.HandgunBeretta:
                case Re3ItemIds.ShotgunBenelli:
                case Re3ItemIds.MagnumSW:
                case Re3ItemIds.GrenadeLauncherGrenade:
                case Re3ItemIds.RocketLauncher:
                case Re3ItemIds.MineThrower:
                case Re3ItemIds.HangunEagle:
                case Re3ItemIds.ShotgunM37:
                case Re3ItemIds.HandgunAmmo:
                case Re3ItemIds.MagnumAmmo:
                case Re3ItemIds.ShotgunAmmo:
                case Re3ItemIds.GrenadeRounds:
                case Re3ItemIds.MineThrowerAmmo:
                case Re3ItemIds.FirstAidSprayBox:
                    return Green | Digit;
                case Re3ItemIds.GrenadeLauncherFlame:
                case Re3ItemIds.HandgunSigproEnhanced:
                case Re3ItemIds.HandgunBerettaEnhanced:
                case Re3ItemIds.ShotgunBenelliEnhanced:
                case Re3ItemIds.MineThrowerEnhanced:
                case Re3ItemIds.FlameRounds:
                case Re3ItemIds.HandgunEnhancedAmmo:
                case Re3ItemIds.ShotgunEnhancedAmmo:
                    return Red | Digit;
                case Re3ItemIds.RifleAmmo:
                    return Red | Percent;
                case Re3ItemIds.RifleM4A1Manual:
                    return Green | Percent;
                case Re3ItemIds.RifleM4A1Auto:
                    return Automatic | Red | Percent;
                case Re3ItemIds.GrenadeLauncherAcid:
                case Re3ItemIds.AcidRounds:
                    return Orange | Digit;
                case Re3ItemIds.GrenadeLauncherFreeze:
                case Re3ItemIds.FreezeRounds:
                    return Blue | Digit;
                case Re3ItemIds.GatlingGun:
                    return Green | Infinity;
                case Re3ItemIds.InkRibbon:
                    return Blue | Digit;
                default:
                    return None;
            }
        }

        protected override void ReplaceTitleCardSound(FileRepository fileRepository, string sourcePath)
        {
            var filenames = new[] {
                "DATA/SOUND/C_00.VB",
                "DATA/SOUND/C_01.VB"
            };
            foreach (var filename in filenames)
            {
                var vbSourcePath = fileRepository.GetDataPath(filename);
                var vbSourceFile = fileRepository.GetBytes(vbSourcePath);
                var vbTargetPath = fileRepository.GetModPath(filename);
                Directory.CreateDirectory(Path.GetDirectoryName(vbTargetPath));

                var waveBuilder = new WaveformBuilder(channels: 1, sampleRate: 32000);
                waveBuilder.Append(sourcePath);
                var pcmData = waveBuilder.GetPCM();

                var vb = new VabFile(vbSourceFile);
                vb.SetSampleFromPCM(5, pcmData);
                vb.SetSampleFromPCM(6, pcmData);
                vb.Write(vbTargetPath);
            }
        }

        private static readonly string[] g_trackOrder = new[]
        {
            "MAIN00",
            "MAIN01",
            "MAIN02",
            "MAIN03",
            "MAIN04",
            "MAIN05",
            "MAIN06",
            "MAIN07",
            "MAIN08",
            "OTHER_00",
            "MAIN0A",
            "MAIN0B",
            "MAIN0B",
            "MAIN0D",
            "MAIN0E",
            "MAIN0F_0",
            "SBB_17",
            "MAIN11",
            "MAIN05",
            "MAIN13",
            "MAIN14",
            "MAIN0D",
            "MAIN16_0",
            "MAIN17",
            "MAIN18",
            "MAIN19",
            "MAIN1A",
            "MAIN1B",
            "OTHER_00",
            "MAIN1D",
            "OTHER_00",
            "OTHER_00",
            "MAIN20_0",
            "MAIN21",
            "OTHER_00",
            "MAIN23",
            "MAIN24_0",
            "OTHER_01",
            "OTHER_00",
            "OTHER_00",
            "MAIN05",
            "MAIN29",
            "MAIN2A_0",
            "MAIN2B_0",
            "MAIN2C",
            "MAIN2D",
            "OTHER_01",
            "OTHER_01",
            "MAIN30_0",
            "OTHER_00",
            "MAIN32",
            "MAIN33",
            "MAIN34",
            "MAIN35",
            "MAIN36",
            "MAIN37",
            "MAIN38",
            "MAIN39",
            "MAIN3A",
            "MAIN3B",
            "MAIN3C",
            "MAIN3D",
            "MAIN3E",
            "MAIN3F",
            "SBB_00",
            "SBB_02",
            "SBB_03",
            "SBB_05",
            "MAIN0b",
            "MAIN04",
            "SBB_09",
            "",
            "OTHER_00",
            "OTHER_01",
            "SBB_0E",
            "SBB_12",
            "MAIN14",
            "MAIN04",
            "SBB_17",
            "SBB_3F",
            "SBB_1B",
            "SBB_1C",
            "SBB_33",
            "SBB_27",
            "SBB_20",
            "OTHER_00",
            "MAIN01",
            "SBB_23",
            "SBB_25",
            "MAIN23",
            "SBB_00",
            "SBB_2A",
            "SBB_44",
            "SBB_1E_0",
            "SBB_2E_0",
            "SBB_1A",
            "SBB_30",
            "SBB_44",
            "SBB_36",
            "SBB_3D",
            "SBB_31",
            "SBB_39",
            "SBB_35",
            "SBB_3B",
            "MAIN14",
            "OTHER_01",
            "SBB_41",
            "SBB_10",
            "OTHER_03",
            "SBB_01",
            "",
            "MAIN21",
            "SBB_06_0",
            "",
            "",
            "SBB_0A",
            "SBB_0C",
            "",
            "MAIN04",
            "SBB_0F",
            "",
            "SBB_01",
            "SBB_16",
            "SBB_18",
            "SBB_40",
            "",
            "SBB_1D",
            "SBB_3A",
            "SBB_11",
            "",
            "",
            "",
            "SBB_24",
            "",
            "",
            "SBB_29",
            "SBB_2B",
            "SBB_01",
            "SBB_1F",
            "SBB_2F",
            "",
            "",
            "SBB_29",
            "",
            "SBB_3E",
            "SBB_38_1",
            "SBB_37",
            "SBB_34",
            "SBB_3C",
            "",
            "",
            "SBB_42",
            "SBB_19",
            "SBB_44"
        };
    }
}
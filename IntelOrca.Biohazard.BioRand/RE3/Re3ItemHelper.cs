﻿using System;
using IntelOrca.Biohazard.Script;

namespace IntelOrca.Biohazard.BioRand.RE3
{
    internal class Re3ItemHelper : IItemHelper
    {
        public byte[] GetAmmoTypeForWeapon(byte type)
        {
            switch (type)
            {
                case Re3ItemIds.HandgunSigpro:
                case Re3ItemIds.HandgunSigproEnhanced:
                case Re3ItemIds.HandgunBeretta:
                case Re3ItemIds.HandgunBerettaEnhanced:
                    return new byte[] {
                        Re3ItemIds.HandgunAmmo,
                        Re3ItemIds.HandgunEnhancedAmmo
                    };
                case Re3ItemIds.HangunEagle:
                    return new byte[] { Re3ItemIds.HandgunAmmo };
                case Re3ItemIds.ShotgunBenelli:
                case Re3ItemIds.ShotgunBenelliEnhanced:
                    return new byte[] { Re3ItemIds.ShotgunAmmo, Re3ItemIds.ShotgunEnhancedAmmo };
                case Re3ItemIds.MagnumSW:
                    return new byte[] { Re3ItemIds.MagnumAmmo };
                case Re3ItemIds.GrenadeLauncherGrenade:
                case Re3ItemIds.GrenadeLauncherFlame:
                case Re3ItemIds.GrenadeLauncherAcid:
                case Re3ItemIds.GrenadeLauncherFreeze:
                    return new[] {
                        Re3ItemIds.AcidRounds,
                        Re3ItemIds.FlameRounds,
                        Re3ItemIds.FreezeRounds,
                        Re3ItemIds.GrenadeRounds
                    };
                case Re3ItemIds.MineThrower:
                case Re3ItemIds.MineThrowerEnhanced:
                    return new byte[] { Re3ItemIds.MineThrowerAmmo };
                case Re3ItemIds.RifleM4A1Manual:
                case Re3ItemIds.RifleM4A1Auto:
                    return new byte[] { Re3ItemIds.RifleAmmo };
                case Re3ItemIds.ShotgunM37:
                    return new byte[] { Re3ItemIds.ShotgunAmmo };
                default:
                    return new byte[0];
            }
        }

        public byte[] GetDefaultWeapons(RandoConfig config)
        {
            return new[] { Re3ItemIds.HandgunBeretta };
        }

        public byte[] GetInitialKeyItems(RandoConfig config)
        {
            return new byte[0];
        }

        public int[]? GetInventorySize(RandoConfig config)
        {
            return new[] { 8, 8 };
        }

        public byte GetItemId(CommonItemKind kind)
        {
            switch (kind)
            {
                case CommonItemKind.HandgunAmmo:
                    return Re3ItemIds.HandgunAmmo;
                case CommonItemKind.InkRibbon:
                    return Re3ItemIds.InkRibbon;
                case CommonItemKind.HerbG:
                    return Re3ItemIds.HerbG;
                case CommonItemKind.HerbGG:
                    return Re3ItemIds.HerbGG;
                case CommonItemKind.HerbGGG:
                    return Re3ItemIds.HerbGGG;
                case CommonItemKind.HerbR:
                    return Re3ItemIds.HerbR;
                case CommonItemKind.HerbGR:
                    return Re3ItemIds.HerbGR;
                case CommonItemKind.HerbB:
                    return Re3ItemIds.HerbB;
                case CommonItemKind.HerbGB:
                    return Re3ItemIds.HerbGB;
                case CommonItemKind.HerbGGB:
                    return Re3ItemIds.HerbGGB;
                case CommonItemKind.HerbGRB:
                    return Re3ItemIds.HerbGRB;
                case CommonItemKind.FirstAid:
                    return Re3ItemIds.FirstAidSpray;
                case CommonItemKind.Knife:
                    return Re3ItemIds.CombatKnife;
            }
            throw new NotImplementedException();
        }

        public string GetItemName(byte type)
        {
            var name = new Bio3ConstantTable().GetItemName(type);
            return name
                .Remove(0, 5)
                .Replace("_", " ");
        }

        public double GetItemProbability(byte type)
        {
            switch (type)
            {
                case Re3ItemIds.HandgunAmmo:
                    return 0.3;
                case Re3ItemIds.ShotgunAmmo:
                case Re3ItemIds.RifleAmmo:
                case Re3ItemIds.HandgunEnhancedAmmo:
                case Re3ItemIds.ShotgunEnhancedAmmo:
                    return 0.2;
                case Re3ItemIds.AcidRounds:
                case Re3ItemIds.FlameRounds:
                case Re3ItemIds.FreezeRounds:
                case Re3ItemIds.GrenadeRounds:
                case Re3ItemIds.MineThrowerAmmo:
                case Re3ItemIds.MagnumAmmo:
                    return 0.1;
                case Re3ItemIds.GunpowderA:
                case Re3ItemIds.GunpowderB:
                case Re3ItemIds.GunpowderC:
                    return 0.5;
                case Re3ItemIds.GunpowderAA:
                case Re3ItemIds.GunpowderBB:
                case Re3ItemIds.GunpowderAC:
                case Re3ItemIds.GunpowderBC:
                case Re3ItemIds.GunpowderCC:
                    return 0.25;
                case Re3ItemIds.GunpowderAAA:
                case Re3ItemIds.GunpowderAAB:
                case Re3ItemIds.GunpowderBBA:
                case Re3ItemIds.GunpowderBBB:
                case Re3ItemIds.GunpowderCCC:
                    return 0.1;
                default:
                    return 0;
            }
        }

        public int GetItemQuantity(RandoConfig config, byte item)
        {
            return 1;
        }

        public byte GetItemSize(byte type)
        {
            return 1;
        }

        public byte GetMaxAmmoForAmmoType(byte type)
        {
            switch (type)
            {
                default:
                    return 1;
                case Re3ItemIds.InkRibbon:
                    return 3;
                case Re3ItemIds.HandgunSigpro:
                case Re3ItemIds.HandgunSigproEnhanced:
                case Re3ItemIds.HandgunBeretta:
                case Re3ItemIds.HandgunBerettaEnhanced:
                case Re3ItemIds.HangunEagle:
                    return 15;
                case Re3ItemIds.ShotgunBenelli:
                case Re3ItemIds.ShotgunBenelliEnhanced:
                    return 7;
                case Re3ItemIds.MagnumSW:
                    return 6;
                case Re3ItemIds.GrenadeLauncherAcid:
                case Re3ItemIds.GrenadeLauncherFlame:
                case Re3ItemIds.GrenadeLauncherFreeze:
                case Re3ItemIds.GrenadeLauncherGrenade:
                    return 6;
                case Re3ItemIds.RocketLauncher:
                    return 4;
                case Re3ItemIds.RifleM4A1Manual:
                case Re3ItemIds.RifleM4A1Auto:
                    return 100;
                case Re3ItemIds.ShotgunM37:
                    return 6;
                case Re3ItemIds.MineThrower:
                case Re3ItemIds.MineThrowerEnhanced:
                    return 6;
                case Re3ItemIds.HandgunAmmo:
                    return 60;
                case Re3ItemIds.ShotgunAmmo:
                case Re3ItemIds.HandgunEnhancedAmmo:
                case Re3ItemIds.ShotgunEnhancedAmmo:
                    return 30;
                case Re3ItemIds.AcidRounds:
                case Re3ItemIds.FlameRounds:
                case Re3ItemIds.FreezeRounds:
                case Re3ItemIds.GrenadeRounds:
                case Re3ItemIds.MineThrowerAmmo:
                case Re3ItemIds.MagnumAmmo:
                    return 10;
                case Re3ItemIds.RifleAmmo:
                    return 100;
            }
        }

        public WeaponKind GetWeaponKind(byte item)
        {
            switch (item)
            {
                case Re3ItemIds.HandgunSigpro:
                case Re3ItemIds.HandgunSigproEnhanced:
                case Re3ItemIds.HandgunBeretta:
                case Re3ItemIds.HandgunBerettaEnhanced:
                case Re3ItemIds.HangunEagle:
                    return WeaponKind.Sidearm;
                case Re3ItemIds.ShotgunBenelli:
                case Re3ItemIds.ShotgunBenelliEnhanced:
                case Re3ItemIds.ShotgunM37:
                case Re3ItemIds.RifleM4A1Manual:
                case Re3ItemIds.RifleM4A1Auto:
                    return WeaponKind.Primary;
                case Re3ItemIds.MagnumSW:
                case Re3ItemIds.GrenadeLauncherAcid:
                case Re3ItemIds.GrenadeLauncherFlame:
                case Re3ItemIds.GrenadeLauncherFreeze:
                case Re3ItemIds.GrenadeLauncherGrenade:
                case Re3ItemIds.MineThrower:
                case Re3ItemIds.MineThrowerEnhanced:
                    return WeaponKind.Powerful;
                default:
                    return WeaponKind.None;
            }
        }

        public byte[] GetWeapons(Rng rng, RandoConfig config)
        {
            return new[] {
                rng.NextOf(Re3ItemIds.HandgunSigpro, Re3ItemIds.HandgunSigproEnhanced),
                rng.NextOf(Re3ItemIds.HandgunBeretta, Re3ItemIds.HandgunBerettaEnhanced),
                rng.NextOf(Re3ItemIds.ShotgunBenelli, Re3ItemIds.ShotgunBenelliEnhanced),
                Re3ItemIds.MagnumSW,
                rng.NextOf(
                    Re3ItemIds.GrenadeLauncherAcid,
                    Re3ItemIds.GrenadeLauncherFlame,
                    Re3ItemIds.GrenadeLauncherFreeze,
                    Re3ItemIds.GrenadeLauncherGrenade),
                Re3ItemIds.RocketLauncher,
                rng.NextOf(Re3ItemIds.MineThrower, Re3ItemIds.MineThrowerEnhanced),
                Re3ItemIds.HangunEagle,
                Re3ItemIds.RifleM4A1Auto,
                Re3ItemIds.ShotgunM37
            };
        }

        public byte? GetWeaponUpgrade(byte weapon, Rng rng, RandoConfig config)
        {
            return null;
        }

        public byte[] GetWeaponGunpowder(byte weapon)
        {
            switch (weapon)
            {
                case Re3ItemIds.HandgunSigpro:
                case Re3ItemIds.HandgunSigproEnhanced:
                case Re3ItemIds.HandgunBeretta:
                case Re3ItemIds.HandgunBerettaEnhanced:
                case Re3ItemIds.HangunEagle:
                    return new[] {
                        Re3ItemIds.GunpowderA,
                        Re3ItemIds.GunpowderAA,
                        Re3ItemIds.GunpowderB,
                        Re3ItemIds.GunpowderBB
                    };
                case Re3ItemIds.GrenadeLauncherAcid:
                case Re3ItemIds.GrenadeLauncherFlame:
                case Re3ItemIds.GrenadeLauncherFreeze:
                case Re3ItemIds.GrenadeLauncherGrenade:
                    return new[] {
                        Re3ItemIds.GunpowderA,
                        Re3ItemIds.GunpowderAA,
                        Re3ItemIds.GunpowderB,
                        Re3ItemIds.GunpowderBB,
                        Re3ItemIds.GunpowderC,
                        Re3ItemIds.GunpowderCC
                    };
                case Re3ItemIds.MagnumSW:
                    return new[] {
                        Re3ItemIds.GunpowderC,
                        Re3ItemIds.GunpowderCC
                    };
                default:
                    return new byte[0];
            }
        }

        public bool HasInkRibbons(RandoConfig config)
        {
            return true;
        }

        public bool HasGunPowder(RandoConfig config) => true;

        public ItemAttribute GetItemAttributes(byte item)
        {
            switch (item)
            {
                case 0:
                    return 0;
                case Re3ItemIds.CombatKnife:
                case Re3ItemIds.HandgunSigpro:
                case Re3ItemIds.HandgunBeretta:
                case Re3ItemIds.ShotgunBenelli:
                case Re3ItemIds.MagnumSW:
                case Re3ItemIds.GrenadeLauncherGrenade:
                case Re3ItemIds.GrenadeLauncherFlame:
                case Re3ItemIds.GrenadeLauncherAcid:
                case Re3ItemIds.GrenadeLauncherFreeze:
                case Re3ItemIds.RocketLauncher:
                case Re3ItemIds.GatlingGun:
                case Re3ItemIds.MineThrower:
                case Re3ItemIds.HangunEagle:
                case Re3ItemIds.RifleM4A1Manual:
                case Re3ItemIds.RifleM4A1Auto:
                case Re3ItemIds.ShotgunM37:
                case Re3ItemIds.HandgunSigproEnhanced:
                case Re3ItemIds.HandgunBerettaEnhanced:
                case Re3ItemIds.ShotgunBenelliEnhanced:
                    return ItemAttribute.Weapon;
                case Re3ItemIds.MineThrowerEnhanced:
                case Re3ItemIds.HandgunAmmo:
                case Re3ItemIds.MagnumAmmo:
                case Re3ItemIds.ShotgunAmmo:
                case Re3ItemIds.GrenadeRounds:
                case Re3ItemIds.FlameRounds:
                case Re3ItemIds.AcidRounds:
                case Re3ItemIds.FreezeRounds:
                case Re3ItemIds.MineThrowerAmmo:
                case Re3ItemIds.RifleAmmo:
                case Re3ItemIds.HandgunEnhancedAmmo:
                case Re3ItemIds.ShotgunEnhancedAmmo:
                    return ItemAttribute.Ammo;
                case Re3ItemIds.FirstAidSpray:
                case Re3ItemIds.HerbG:
                case Re3ItemIds.HerbB:
                case Re3ItemIds.HerbR:
                case Re3ItemIds.HerbGG:
                case Re3ItemIds.HerbGB:
                case Re3ItemIds.HerbGR:
                case Re3ItemIds.HerbGGG:
                case Re3ItemIds.HerbGGB:
                case Re3ItemIds.HerbGRB:
                case Re3ItemIds.FirstAidSprayBox:
                    return ItemAttribute.Heal;
                case Re3ItemIds.SquareCrank:
                case Re3ItemIds.UnknownRedMedal:
                case Re3ItemIds.UnknownBlueMedal:
                case Re3ItemIds.UnknownGoldMedal:
                case Re3ItemIds.JillSTARSCard:
                case Re3ItemIds.UnknownOilCanGigaOil:
                case Re3ItemIds.Battery:
                case Re3ItemIds.Firehook:
                case Re3ItemIds.PowerCable:
                case Re3ItemIds.Fuse:
                case Re3ItemIds.UnknownBrokenFirehose:
                case Re3ItemIds.OilAdditive:
                case Re3ItemIds.BradVickersCardcase:
                case Re3ItemIds.BradVickersSTARSCard:
                case Re3ItemIds.MachineOil:
                case Re3ItemIds.MixedOil:
                case Re3ItemIds.UnknownSteelChain:
                case Re3ItemIds.Wrench:
                case Re3ItemIds.IronPipe:
                case Re3ItemIds.UnknownCylinder:
                case Re3ItemIds.Firehose:
                case Re3ItemIds.TapeRecorder:
                case Re3ItemIds.LighterOil:
                case Re3ItemIds.LighterNoOil:
                case Re3ItemIds.Lighter:
                case Re3ItemIds.GreenGem:
                case Re3ItemIds.BlueGem:
                case Re3ItemIds.AmberBall:
                case Re3ItemIds.ObsidianBall:
                case Re3ItemIds.CrystalBall:
                case Re3ItemIds.UnknownRemoteControlNoBatteries:
                case Re3ItemIds.UnknownRemoteControlBatteries:
                case Re3ItemIds.UnknownAABatteries:
                case Re3ItemIds.GoldGear:
                case Re3ItemIds.SilverGear:
                case Re3ItemIds.ChronosGear:
                case Re3ItemIds.BronzeBook:
                case Re3ItemIds.BronzeCompass:
                case Re3ItemIds.VaccineMedium:
                case Re3ItemIds.VaccineBase:
                    return ItemAttribute.Key;
                case Re3ItemIds.UnknownSigproSP2009handgun53:
                case Re3ItemIds.UnknownSigproSP2009handgun54:
                    return ItemAttribute.Weapon;
                case Re3ItemIds.Vaccine:
                    return ItemAttribute.Key;
                case Re3ItemIds.UnknownSigproSP2009handgun56:
                case Re3ItemIds.UnknownSigproSP2009handgun57:
                    return ItemAttribute.Weapon;
                case Re3ItemIds.MediumBase:
                    return ItemAttribute.Key;
                case Re3ItemIds.EaglePartsA:
                case Re3ItemIds.EaglePartsB:
                case Re3ItemIds.M37PartsA:
                case Re3ItemIds.M37PartsB:
                case Re3ItemIds.UnknownSigproSP2009handgun5D:
                    return ItemAttribute.Weapon;
                case Re3ItemIds.Chronoschain:
                case Re3ItemIds.Rustedcrank:
                case Re3ItemIds.Cardkey:
                    return ItemAttribute.Key;
                case Re3ItemIds.GunpowderA:
                case Re3ItemIds.GunpowderB:
                case Re3ItemIds.GunpowderC:
                case Re3ItemIds.GunpowderAA:
                case Re3ItemIds.GunpowderBB:
                case Re3ItemIds.GunpowderAC:
                case Re3ItemIds.GunpowderBC:
                case Re3ItemIds.GunpowderCC:
                case Re3ItemIds.GunpowderAAA:
                case Re3ItemIds.GunpowderAAB:
                case Re3ItemIds.GunpowderBBA:
                case Re3ItemIds.GunpowderBBB:
                case Re3ItemIds.GunpowderCCC:
                    return ItemAttribute.Gunpowder;
                case Re3ItemIds.InfiniteBullets:
                    return ItemAttribute.Special;
                case Re3ItemIds.WaterSample:
                case Re3ItemIds.SystemDisk:
                case Re3ItemIds.DummyKey:
                case Re3ItemIds.Lockpick:
                case Re3ItemIds.WarehouseKeyBackdoor:
                case Re3ItemIds.SickroomKeyRoom402:
                case Re3ItemIds.EmblemKeySTARS:
                case Re3ItemIds.UnknownKeyringWith4UnknownKeys:
                case Re3ItemIds.ClockTowerKeyBezel:
                case Re3ItemIds.ClockTowerKeyWinder:
                case Re3ItemIds.ChronosKey:
                    return ItemAttribute.Key;
                case Re3ItemIds.UnknownSigproSP2009handgun7A:
                    return ItemAttribute.Weapon;
                case Re3ItemIds.ParkKeyFront:
                case Re3ItemIds.ParkKeyGraveyard:
                case Re3ItemIds.ParkKeyRear:
                case Re3ItemIds.FacilityKeyNobarcode:
                case Re3ItemIds.FacilityKeyBarcode:
                case Re3ItemIds.BoutiqueKey:
                    return ItemAttribute.Key;
                case Re3ItemIds.InkRibbon:
                    return ItemAttribute.InkRibbon;
                case Re3ItemIds.ReloadingTool:
                    return ItemAttribute.Special;
                case Re3ItemIds.GameInstructionsA:
                case Re3ItemIds.GameInstructionsB:
                case Re3ItemIds.GameInstructionsA2:
                default:
                    return ItemAttribute.Document;
            }
        }

        public bool IsItemInfinite(byte type)
        {
            return false;
        }

        public bool IsRe2ItemIdsDiscardable(byte type)
        {
            switch (type)
            {
                case Re3ItemIds.Lockpick:
                case Re3ItemIds.WarehouseKeyBackdoor:
                case Re3ItemIds.SickroomKeyRoom402:
                case Re3ItemIds.EmblemKeySTARS:
                case Re3ItemIds.ClockTowerKeyBezel:
                case Re3ItemIds.ClockTowerKeyWinder:
                case Re3ItemIds.ChronosKey:
                case Re3ItemIds.ParkKeyFront:
                case Re3ItemIds.ParkKeyGraveyard:
                case Re3ItemIds.ParkKeyRear:
                case Re3ItemIds.FacilityKeyNobarcode:
                case Re3ItemIds.FacilityKeyBarcode:
                case Re3ItemIds.BoutiqueKey:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsOptionalItem(RandoConfig config, byte type)
        {
            return false;
        }

        public bool IsWeaponCompatible(byte player, byte item)
        {
            return true;
        }
    }
}

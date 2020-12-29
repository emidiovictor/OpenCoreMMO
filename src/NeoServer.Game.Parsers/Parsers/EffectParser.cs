﻿using NeoServer.Enums.Creatures.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoServer.Game.Effects.Parsers
{
    public class EffectParser
    {
        public static EffectT Parse(string effect)
        {
            return effect switch
            {
                "redspark" => EffectT.XBlood,
                "bluebubble" => EffectT.BubbleBlue,
                "poff" => EffectT.Puff,
                "yellowspark" => EffectT.SparkYellow,
                "explosionarea" => EffectT.DamageExplosion,
                "explosion" => EffectT.DamageMagicMissile,
                "firearea" => EffectT.AreaFlame,
                "yellowbubble" => EffectT.RingsYellow,
                "greenbubble" => EffectT.RingsGreen,
                "blackspark" => EffectT.XGray,
                "teleport" => EffectT.BubbleBlue,
                "energy" => EffectT.DamageEnergy,
                "blueshimmer" => EffectT.GlitterBlue,
                "redshimmer" => EffectT.GlitterRed,
                "greenshimmer" => EffectT.GlitterGreen,
                "fire" => EffectT.Flame,
                "greenspark" => EffectT.XPoison,
                "mortarea" => EffectT.BubbleBlack,
                "greennote" => EffectT.SoundGreen,
                "rednote" => EffectT.SoundRed,
                "poison" => EffectT.DamageVenomMissile,
                "yellownote" => EffectT.SoundYellow,
                "purplenote" => EffectT.SoundPurple,
                "bluenote" => EffectT.SoundBlue,
                "whitenote" => EffectT.SoundWhite,
                "bubbles" => EffectT.Bubbles,
                "dice" => EffectT.Craps,
                "giftwraps" => EffectT.GiftWraps,
                "yellowfirework" => EffectT.FireworkYellow,
                "redfirework" => EffectT.FireworkRed,
                "bluefirework" => EffectT.FireworkBlue,
                "stun" => EffectT.Stun,
                "sleep" => EffectT.Sleep,
                "watercreature" => EffectT.Watercreature,
                "groundshaker" => EffectT.GroundShaker,
                "hearts" => EffectT.Hearts,
                "fireattack" => EffectT.Fireattack,
                "energyarea" => EffectT.Energyarea,
                "smallclouds" => EffectT.Smallclouds,
                "holydamage" => EffectT.Holydamage,
                "bigclouds" => EffectT.BigClouds,
                "icearea" => EffectT.IceArea,
                "icetornado" => EffectT.IceRornado,
                "iceattack" => EffectT.IceAttack,
                "stones" => EffectT.Stones,
                "smallplants" => EffectT.SmallPlants,
                "carniphila" => EffectT.Carniphila,
                "purpleenergy" => EffectT.PurpleEnergy,
                "yellowenergy" => EffectT.YellowEnergy,
                "holyarea" => EffectT.HolyArea,
                "bigplants" => EffectT.Bigplants,
                "cake" => EffectT.Cake,
                "giantice" => EffectT.Giantice,
                "watersplash" => EffectT.Watersplash,
                "plantattack" => EffectT.Plantattack,
                "tutorialarrow" => EffectT.Tutorialarrow,
                "tutorialsquare" => EffectT.Tutorialsquare,
                "mirrorhorizontal" => EffectT.Mirrorhorizontal,
                "mirrorvertical" => EffectT.Mirrorvertical,
                "skullhorizontal" => EffectT.Skullhorizontal,
                "skullvertical" => EffectT.Skullvertical,
                "assassin" => EffectT.Assassin,
                "stepshorizontal" => EffectT.Stepshorizontal,
                "bloodysteps" => EffectT.Bloodysteps,
                "stepsvertical" => EffectT.Stepsvertical,
                "yalaharighost" => EffectT.Yalaharighost,
                "bats" => EffectT.Bats,
                "smoke" => EffectT.Smoke,
                "insects" => EffectT.Insects,
                "dragonhead" => EffectT.Dragonhead,
                _ => EffectT.None
            };
        }
    }
}

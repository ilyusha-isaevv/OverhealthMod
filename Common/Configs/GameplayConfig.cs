using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace OverhealthMod.Common.Configs;

public class GameplayConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Range(10f, 100f)]
    [Increment(5f)]
    [DefaultValue(100f)]
    public float MaximumOverhealth { get; set; }

    [Header("Decay")]
    [Range(1f, 10f)]
    [Increment(0.1f)]
    [DefaultValue(1.3f)]
    public float ConstantDecayPercent { get; set; }

    [Range(5f, 20f)]
    [Increment(0.1f)]
    [DefaultValue(6f)]
    public float ProgressiveDecayPercent { get; set; }

    [Range(1f, 3f)]
    [Increment(0.1f)]
    [DefaultValue(1f)]
    public float DecayMutiplier { get; set; }
}

using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace OverhealthMod.Common.Configs;

public class GameplayConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Range(1f, 10f)]
    [Increment(0.1f)]
    [DefaultValue(1.2f)]
    public float ConstantDecreaseRate { get; set; }

    [Range(5f, 20f)]
    [Increment(0.1f)]
    [DefaultValue(5.8f)]
    public float ProgressiveDecreaseRate { get; set; }

    [Range(1f, 3f)]
    [Increment(0.1f)]
    [DefaultValue(1f)]
    public float OverhealthDecreaseRateMutiplier { get; set; }
}

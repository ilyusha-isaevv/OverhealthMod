using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace OverhealthMod.Common.Configs;

public class GameplayConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Range(1f, 3f)]
    [DefaultValue(1f)]
    public float OverhealthDecreaseRateMutiplier { get; set; }

    [Range(1.5f, 10f)]
    [DefaultValue(2f)]
    public float ConstantDecreaseRate { get; set; }

    [Range(4f, 20f)]
    [DefaultValue(5f)]
    public float ProgressiveDecreaseRate { get; set; }
}

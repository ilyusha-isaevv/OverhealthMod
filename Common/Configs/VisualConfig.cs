using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace OverhealthMod.Common.Configs;

public class VisualConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [DefaultValue(true)]
    public bool ShowOverhealthUI { get; set; }

    [Range(0f, 1f)]
    [Increment(0.05f)]
    [DefaultValue(0.6f)]
    public float ClassicDisplaySetOpacity { get; set; }

    [Range(0f, 1f)]
    [Increment(0.05f)]
    [DefaultValue(0.6f)]
    public float FancyDisplaySetOpacity { get; set; }

    [Range(0f, 1f)]
    [Increment(0.05f)]
    [DefaultValue(1f)]
    public float HorizontalBarsDisplaySetOpacity { get; set; }

    [Range(0f, 1f)]
    [Increment(0.05f)]
    [DefaultValue(0.7f)]
    public float MultiplayerHealthBarOpacity { get; set; }
}

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace OverhealthMod;

[Autoload(Side = ModSide.Client)]
public class OverhealthUI : ModSystem
{
    private Asset<Texture2D> _horizontalBarLeftPanelTexture;
    private Asset<Texture2D> _horizontalBarMiddlePanelTexture;
    private Asset<Texture2D> _classicHeartTexture;
    private Asset<Texture2D> _fancyHeartLeft;
    private Asset<Texture2D> _fancyHeartMiddle;
    private Asset<Texture2D> _fancyHeartRight;
    private Asset<Texture2D> _fancyHeartSingle;
    private Asset<Texture2D> _healthBarTexture;

    public override void Load()
    {
        // Textures
        _horizontalBarLeftPanelTexture = Mod.Assets.Request<Texture2D>("Assets/HorizontalBar/PanelLeft");
        _horizontalBarMiddlePanelTexture = Mod.Assets.Request<Texture2D>("Assets/HorizontalBar/PanelMiddle");
        _classicHeartTexture = Mod.Assets.Request<Texture2D>("Assets/Classic/Heart");
        _fancyHeartLeft = Mod.Assets.Request<Texture2D>("Assets/Fancy/Left");
        _fancyHeartMiddle = Mod.Assets.Request<Texture2D>("Assets/Fancy/Middle");
        _fancyHeartRight = Mod.Assets.Request<Texture2D>("Assets/Fancy/Right");
        _fancyHeartSingle = Mod.Assets.Request<Texture2D>("Assets/Fancy/Single");
        _healthBarTexture = Mod.Assets.Request<Texture2D>("Assets/HealthBar");

        // Vanilla health bar display sets
        On_ClassicPlayerResourcesDisplaySet.DrawLife += DrawClassicOverhealth;
        On_FancyClassicPlayerResourcesDisplaySet.DrawLifeBar += DrawFancyOverhealth;
        On_HorizontalBarsPlayerResourcesDisplaySet.Draw += DrawHorizontalBarsOverhealth;

        // Multiplayer health bars
        IL_Main.DrawInterface_14_EntityHealthBars += ILModify_Main_OtherPlayersHealthbarOverhealth; // Under players
        On_NewMultiplayerClosePlayersOverlay.Draw += MultiplayerHealthbarOverhealth; // Offscreen teammates

    }

    private void ILModify_Main_OtherPlayersHealthbarOverhealth(ILContext il)
    {
        try
        {
            ILCursor c = new(il);
            c.GotoNext(i => i.MatchLdfld<Player>("statLifeMax2")); // if (... && Main.player[k].statLife != Main.player[k].statLifeMax2)
            // Move after DrawHealthBar
            c.GotoNext(MoveType.After, i => i.MatchCall<Main>("DrawHealthBar"));

            c.EmitLdloc(27); // Player index
            c.EmitDelegate((int playerIdx) =>
            {
                Player p = Main.player[playerIdx];
                int overhealth = p.GetModPlayer<OverhealthPlayer>().Overhealth;

                Vector2 pos = p.Bottom;
                pos.Y += 10f + p.gfxOffY;

                float alpha = p.stealth * Lighting.Brightness((int)(p.Center.X / 16f), (int)(p.Center.Y / 16f));
                DrawOverhealthOverHealthBar(pos, overhealth, p.statLifeMax2, alpha, 1f, false);
            });
        }
        catch (Exception)
        {
            MonoModHooks.DumpIL(Mod, il);
        }
    }

    private void MultiplayerHealthbarOverhealth(On_NewMultiplayerClosePlayersOverlay.orig_Draw orig, NewMultiplayerClosePlayersOverlay self)
    {
        orig(self);
        if (Main.teamNamePlateDistance <= 0)
            return;

        // It's impossible to cast object list (object) to list of objects (List<object>), so use IList
        IList offscreenPlayersListObj = (IList)self.GetType().GetField("_playerOffScreenCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
        // Reference for PlayerOffScreenCache struct is unavailable. Getting the type another way
        Type playerOffScreenCacheType = offscreenPlayersListObj.GetType().GetGenericArguments().Single();
        FieldInfo playerField = playerOffScreenCacheType.GetField("player", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo distanceDrawPositionField = playerOffScreenCacheType.GetField("distanceDrawPosition", BindingFlags.NonPublic | BindingFlags.Instance);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
        // Logic taken from System.Void Terraria.GameContent.UI.NewMultiplayerClosePlayersOverlay/PlayerOffScreenCache::DrawLifeBar()
        foreach (object playerCache in offscreenPlayersListObj)
        {
            Player player = (Player)playerField.GetValue(playerCache);
            Vector2 distanceDrawPosition = (Vector2)distanceDrawPositionField.GetValue(playerCache);

            int overhealth = player.GetModPlayer<OverhealthPlayer>().Overhealth;
            DrawOverhealthOverHealthBar(Main.screenPosition + distanceDrawPosition + new Vector2(26f, 20f), overhealth, player.statLifeMax2, 1f, 1.25f, false);
        }
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, Main.UIScaleMatrix);
    }

    private void DrawOverhealthOverHealthBar(Vector2 pos, int overhealth, int maxLife, float alpha, float scale = 1f, bool noFlip = false)
    {
        if (overhealth <= 0) return;

        // Logic taken from System.Void Terraria.Main::DrawHealthBar()
        float num3 = pos.X - 18f * scale;
        float num4 = pos.Y;
        if (Main.LocalPlayer.gravDir == -1f && !noFlip)
        {
            num4 -= Main.screenPosition.Y;
            num4 = Main.screenPosition.Y + Main.screenHeight - num4;
        }

        Vector2 drawPosition = new Vector2(num3 - Main.screenPosition.X, num4 - Main.screenPosition.Y);

        float percentFilled = (float)overhealth / maxLife;
        if (percentFilled > 1f) percentFilled = 1f;

        int widthToDraw = (int)(36f * percentFilled);
        if (widthToDraw <= 0) return;

        Rectangle sourceRect = new(0, 0, widthToDraw, _healthBarTexture.Value.Height); ;

        Main.spriteBatch.Draw(
            _healthBarTexture.Value,
            drawPosition,
            sourceRect,
            Color.White * alpha * 0.7f,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f
        );
    }

    private void DrawClassicOverhealth(On_ClassicPlayerResourcesDisplaySet.orig_DrawLife orig, ClassicPlayerResourcesDisplaySet self)
    {
        orig(self);

        OverhealthPlayer overhealthPlayer = Main.LocalPlayer.GetModPlayer<OverhealthPlayer>();
        int overhealth = overhealthPlayer.Overhealth;
        if (overhealth == 0) return;

        PlayerStatsSnapshot statsSnapshot = new(Main.LocalPlayer);
        float hpSegmentValue = statsSnapshot.LifePerSegment;
        int hpSegments = statsSnapshot.AmountOfLifeHearts;

        int uiScreenAnchorX = (int)self.GetType().GetField("UI_ScreenAnchorX", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(self);

        SpriteBatch spriteBatch = Main.spriteBatch;

        const int vanillaHeartWidth = 11;
        const int vanillaHeartHeight = 11;

        for (int segmentIndex = 1; segmentIndex <= hpSegments; segmentIndex++)
        {
            float percentFilled = (overhealth - (segmentIndex - 1) * hpSegmentValue) / hpSegmentValue;
            if (percentFilled <= 0f) break;

            float scale = 1f;
            float opacity = 0.55f;
            if (percentFilled < 1f)
            {
                scale = percentFilled / 4f + 0.75f; // From vanilla
                opacity = 0.15f + 0.4f * percentFilled;
            }

            int segmentXOffset = 0;
            int segmentYOffset = 0;
            if (segmentIndex > 10)
            {
                segmentXOffset -= 260;
                segmentYOffset += 26;
            }

            // From Terraria.GameContent.UI.ResourceSets.ClassicPlayerResourcesDisplaySet.DrawLife
            Vector2 position = new(
                (float)(500f + 26f * (segmentIndex - 1) + segmentXOffset + uiScreenAnchorX + vanillaHeartWidth / 2f) - 2f,
                32f + segmentYOffset + (float)(vanillaHeartHeight / 2f) - 2f
            );

            spriteBatch.Draw(
                _classicHeartTexture.Value,
                position,
                null,
                Color.White * opacity,
                0f,
                new Vector2(vanillaHeartWidth / 2f, vanillaHeartHeight / 2f),
                scale,
                SpriteEffects.None,
                0f
            );
        }
    }

    private void DrawFancyOverhealth(On_FancyClassicPlayerResourcesDisplaySet.orig_DrawLifeBar orig, FancyClassicPlayerResourcesDisplaySet self, SpriteBatch spriteBatch)
    {
        orig(self, spriteBatch);

        OverhealthPlayer overhealthPlayer = Main.LocalPlayer.GetModPlayer<OverhealthPlayer>();
        int overhealth = overhealthPlayer.Overhealth;
        if (overhealth == 0) return;

        PlayerStatsSnapshot statsSnapshot = new(Main.LocalPlayer);
        float hpSegmentValue = statsSnapshot.LifePerSegment;

        // Retrieve private fields via reflection
        bool drawText = (bool)self.GetType().GetField("_drawText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(self);
        int heartCountRow1 = (int)self.GetType().GetField("_heartCountRow1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(self);
        int heartCountRow2 = (int)self.GetType().GetField("_heartCountRow2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(self);

        Vector2 FirstHeartPos = new(Main.screenWidth - 300 + 4, 15f);
        if (drawText)
            FirstHeartPos.Y += 6f;

        Vector2 pos = FirstHeartPos;
        for (int i = 0; i < heartCountRow1; i++)
        {
            float percentFilled = (overhealth - i * hpSegmentValue) / hpSegmentValue;
            if (percentFilled <= 0f) break;

            float opacity = 0.6f;
            if (percentFilled < 1f)
                opacity = 0.1f + 0.5f * percentFilled;

            Asset<Texture2D> texture = _fancyHeartMiddle;
            if (heartCountRow1 == 1)
                texture = _fancyHeartSingle;
            else if (i == 0)
                texture = _fancyHeartLeft;
            else if (i == heartCountRow1 - 1)
                texture = _fancyHeartRight;

            spriteBatch.Draw(
                texture.Value,
                pos,
                null,
                Color.White * opacity,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );

            pos.X += texture.Value.Width;
        }

        pos = new Vector2(FirstHeartPos.X, FirstHeartPos.Y + 28f);
        for (int i = 0; i < heartCountRow2; i++)
        {
            int globalIndex = 10 + i;
            float percentFilled = (overhealth - globalIndex * hpSegmentValue) / hpSegmentValue;
            if (percentFilled <= 0f) break;

            float opacity = 0.6f;
            if (percentFilled < 1f)
                opacity = 0.1f + 0.5f * percentFilled;

            Asset<Texture2D> texture = _fancyHeartMiddle;
            if (heartCountRow2 == 1)
                texture = _fancyHeartSingle;
            else if (i == 0)
                texture = _fancyHeartLeft;
            else if (i == heartCountRow2 - 1)
                texture = _fancyHeartRight;

            spriteBatch.Draw(
                texture.Value,
                pos,
                null,
                Color.White * opacity,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                0f
            );

            pos.X += texture.Value.Width;
        }
    }

    private void DrawHorizontalBarsOverhealth(On_HorizontalBarsPlayerResourcesDisplaySet.orig_Draw orig, HorizontalBarsPlayerResourcesDisplaySet self)
    {
        orig(self);

        OverhealthPlayer overhealthPlayer = Main.LocalPlayer.GetModPlayer<OverhealthPlayer>();
        int overhealth = overhealthPlayer.Overhealth;
        if (overhealth == 0) return;

        const int maxHpSegments = 20;
        const int middleHpSegmentWidth = 12;
        const int leftHpSegmentWidth = 6;

        PlayerStatsSnapshot statsSnapshot = new(Main.LocalPlayer);
        float hpSegmentValue = statsSnapshot.LifePerSegment;

        byte drawTextStyle = (byte)self.GetType().GetField("_drawTextStyle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(self);

        // From Terraria.GameContent.UI.ResourceSets.HorizontalBarsPlayerResourcesDisplaySet.Draw
        Vector2 topLeft = new(
            Main.screenWidth - 300f - 22f + 16f,
            18f + (drawTextStyle == 2 ? 2f : drawTextStyle == 1 ? 4f : 0f)
        );
        Vector2 topRight = new(
            topLeft.X + leftHpSegmentWidth + middleHpSegmentWidth * maxHpSegments,
            topLeft.Y
        );

        SpriteBatch spriteBatch = Main.spriteBatch;
        // Change BlendState from AlphaBlend to Additive for transparency to work
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, null, Main.UIScaleMatrix);

        int fullSegments = (int)(overhealth / hpSegmentValue);
        for (int i = 0; i < fullSegments + 1; i++)
        {
            Vector2 segmentPos = new(
                topRight.X - (i + 1) * middleHpSegmentWidth,
                topRight.Y
            );

            if (i < fullSegments) // Draw full segment without source rect
                spriteBatch.Draw(_horizontalBarMiddlePanelTexture.Value, segmentPos, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            else
            {
                float percentFilled = overhealth % hpSegmentValue / (float)hpSegmentValue;
                int xOffset = (int)(middleHpSegmentWidth * (1f - percentFilled));
                Rectangle sourceRect = new(
                    xOffset, 0,
                    middleHpSegmentWidth - xOffset, _horizontalBarMiddlePanelTexture.Value.Height
                );
                Vector2 newPos = new(segmentPos.X + xOffset, segmentPos.Y);
                spriteBatch.Draw(_horizontalBarMiddlePanelTexture.Value, newPos, sourceRect, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
    }
}
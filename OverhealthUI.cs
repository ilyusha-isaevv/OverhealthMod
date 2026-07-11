using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace OverhealthMod;

public class OverhealthUI : ModSystem
{
    private Asset<Texture2D> _horizontalBarLeftPanelTexture;
    private Asset<Texture2D> _horizontalBarMiddlePanelTexture;
    private Asset<Texture2D> _classicHeartTexture;
    private Asset<Texture2D> _fancyHeartLeft;
    private Asset<Texture2D> _fancyHeartMiddle;
    private Asset<Texture2D> _fancyHeartRight;
    private Asset<Texture2D> _fancyHeartSingle;

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

        On_ClassicPlayerResourcesDisplaySet.DrawLife += DrawClassicOverhealth;
        On_FancyClassicPlayerResourcesDisplaySet.DrawLifeBar += DrawFancyOverhealth;
        On_HorizontalBarsPlayerResourcesDisplaySet.Draw += DrawHorizontalBarsOverhealth;
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
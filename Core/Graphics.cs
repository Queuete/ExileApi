using ExileCore.RenderQ;
using ExileCore.Shared.AtlasHelper;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;
using Vector2N = System.Numerics.Vector2;

namespace ExileCore
{
    public class Graphics
    {
        private static readonly RectangleF DefaultUV = new RectangleF(0, 0, 1, 1);
        private readonly CoreSettings _settings;
        private readonly ImGuiRender ImGuiRender;
        private readonly SpritesRender SpritesRender;

        public Graphics(DX11 dx11, CoreSettings settings)
        {
            LowLevel = dx11;
            _settings = settings;
            ImGuiRender = dx11.ImGuiRender;
            SpritesRender = dx11.SpritesRender;
        }

        public DX11 LowLevel { get; }
        public bool TransparentState => ImGuiRender.TransparentState;
        public FontContainer Font => ImGuiRender.fonts[_settings.Font];
        public FontContainer LastFont => ImGuiRender.CurrentFont;

        public Vector2N DrawText(string text, Vector2 position, Color color, string fontName = null,
            FontAlign align = FontAlign.Left)
        {
            return ImGuiRender.DrawText(text, position.ToVector2Num(), color, -1, fontName, align);
        }

        public Vector2N DrawText(string text, Vector2N position, Color color, string fontName = null,
            FontAlign align = FontAlign.Left)
        {
            return ImGuiRender.DrawText(text, position, color, -1, fontName, align);
        }

        public Vector2N DrawText(string text, Vector2N position, Color color, int height, FontAlign align = FontAlign.Left)
        {
            return ImGuiRender.DrawText(text, position, color, height, _settings.Font, align);
        }

        public Vector2N DrawText(string text, Vector2N position, Color color, int height, string fontName,
            FontAlign align = FontAlign.Left)
        {
            return ImGuiRender.DrawText(text, position, color, height, fontName, align);
        }

        public Vector2N DrawText(string text, Vector2 position, Color color)
        {
            return DrawText(text, position.ToVector2Num(), color, _settings.FontSize, _settings.Font);
        }

        public Vector2N DrawText(string text, Vector2 position, Color color, int height)
        {
            return DrawText(text, position.ToVector2Num(), color, height, _settings.Font);
        }

        public Vector2N DrawText(string text, Vector2 position, Color color, FontAlign align = FontAlign.Left)
        {
            return DrawText(text, position.ToVector2Num(), color, _settings.FontSize, _settings.Font, align);
        }

        public Vector2N DrawText(string text, Vector2 position, Color color, int height, FontAlign align = FontAlign.Left)
        {
            return DrawText(text, position.ToVector2Num(), color, height, _settings.Font, align);
        }

        public Vector2N DrawText(string text, Vector2N position, Color color)
        {
            return DrawText(text, position, color, _settings.FontSize, _settings.Font);
        }

        public Vector2N DrawText(string text, Vector2N position)
        {
            return DrawText(text, position, Color.White);
        }

        public Vector2N DrawText(string text, Vector2N position, FontAlign align = FontAlign.Left)
        {
            return DrawText(text, position, Color.White, _settings.FontSize, align);
        }

        public Vector2N DrawText(string text, Vector2 position, FontAlign align = FontAlign.Left)
        {
            return DrawText(text, position.ToVector2Num(), Color.White, _settings.FontSize, align);
        }

        public Vector2N MeasureText(string text)
        {
            return ImGuiRender.MeasureText(text).Mult(_settings.FontSpacing, 1);
        }

        public Vector2N MeasureText(string text, int height)
        {
            return ImGuiRender.MeasureText(text, height).Mult(_settings.FontSpacing, 1);
        }

        public void DrawLine(Vector2N p1, Vector2N p2, float borderWidth, Color color)
        {
            ImGuiRender.LowLevelApi.AddLine(p1, p2, color.ToImgui(), borderWidth);
        }

        public void DrawLine(Vector2 p1, Vector2 p2, float borderWidth, Color color)
        {
            ImGuiRender.LowLevelApi.AddLine(p1.ToVector2Num(), p2.ToVector2Num(), color.ToImgui(), borderWidth);
        }

        public void DrawFrame(Vector2N p1, Vector2N p2, Color color, float rounding, int thickness, int flags)
        {
            ImGuiRender.LowLevelApi.AddRect(p1, p2, color.ToImgui(), rounding, flags, thickness);
        }

        public void DrawBox(Vector2N p1, Vector2N p2, Color color, float rounding = 0)
        {
            ImGuiRender.LowLevelApi.AddRectFilled(p1, p2, color.ToImgui(), rounding);
        }

        public void DrawBox(Vector2 p1, Vector2 p2, Color color, float rounding = 0)
        {
            ImGuiRender.LowLevelApi.AddRectFilled(p1.ToVector2Num(), p2.ToVector2Num(), color.ToImgui(), rounding);
        }

        public void DrawTexture(System.IntPtr user_texture_id, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            ImGuiRender.LowLevelApi.AddImageQuad(user_texture_id, a.ToVector2Num(), b.ToVector2Num(), c.ToVector2Num(), d.ToVector2Num());
        }

        public void DrawImage(string fileName, RectangleF rectangle)
        {
            DrawImage(fileName, rectangle, DefaultUV, Color.White);
        }

        public void DrawImage(string fileName, RectangleF rectangle, Color color)
        {
            DrawImage(fileName, rectangle, DefaultUV, color);
        }

        public void DrawImage(string fileName, RectangleF rectangle, RectangleF uv, Color color)
        {
            SpritesRender.DrawImage(fileName, rectangle, uv, color);
        }

        public void DrawImage(string fileName, RectangleF rectangle, RectangleF uv)
        {
            DrawImage(fileName, rectangle, uv, Color.White);
        }

        public void DrawImage(AtlasTexture atlasTexture, RectangleF rectangle)
        {
            DrawImage(atlasTexture, rectangle, Color.White);
        }

        public void DrawImage(AtlasTexture atlasTexture, RectangleF rectangle, Color color)
        {
            SpritesRender.DrawImage(atlasTexture.AtlasFileName, rectangle, atlasTexture.TextureUV, color);
        }

        public void DrawImageGui(string fileName, RectangleF rectangle, RectangleF uv)
        {
            ImGuiRender.DrawImage(fileName, rectangle, uv);
        }

        public void DrawImageGui(string fileName, Vector2N TopLeft, Vector2N BottomRight, Vector2N TopLeft_UV, Vector2N BottomRight_UV)
        {
            ImGuiRender.DrawImage(fileName, TopLeft, BottomRight, TopLeft_UV, BottomRight_UV);
        }

        public void DrawBox(RectangleF rect, Color color)
        {
            DrawBox(rect, color, 0);
        }

        public void DrawBox(RectangleF rect, Color color, float rounding)
        {
            DrawBox(rect.TopLeft.ToVector2Num(), rect.BottomRight.ToVector2Num(), color, rounding);
        }

        public void DrawFrame(RectangleF rect, Color color, float rounding, int thickness, int flags)
        {
            DrawFrame(rect.TopLeft.ToVector2Num(), rect.BottomRight.ToVector2Num(), color, rounding, thickness, flags);
        }

        public void DrawFrame(RectangleF rect, Color color, int thickness)
        {
            DrawFrame(rect.TopLeft.ToVector2Num(), rect.BottomRight.ToVector2Num(), color, 0, thickness, 0);
        }

        public void DrawFrame(Vector2 p1, Vector2 p2, Color color, int thickness)
        {
            DrawFrame(p1.ToVector2Num(), p2.ToVector2Num(), color, 0, thickness, 0);
        }

        public bool InitImage(string name, bool textures = true)
        {
            return SpritesRender.LoadPng(textures ? $"textures/{name}" : name)!=null;
        }

        public void DisposeTexture(string name)
        {
            LowLevel.DisposeTexture(name);
        }
    }
}

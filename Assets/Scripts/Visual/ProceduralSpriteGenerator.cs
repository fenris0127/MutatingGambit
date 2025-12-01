using UnityEngine;
using MutatingGambit.Core.ChessEngine;

namespace MutatingGambit.Visual
{
    /// <summary>
    /// 기물을 위한 간단한 시각적 표현을 생성합니다.
    /// 프로토타입용 - 각 기물 타입을 기본 도형으로 표현합니다.
    /// </summary>
    public class ProceduralSpriteGenerator
    {
        #region 상수
        private const int SPRITE_SIZE = 64;
        private const float SPRITE_PIVOT_X = 0.5f;
        private const float SPRITE_PIVOT_Y = 0.5f;
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 기물 타입에 맞는 프로시저럴 스프라이트를 생성합니다.
        /// </summary>
        /// <param name="type">기물 타입</param>
        /// <returns>생성된 스프라이트</returns>
        public Sprite GenerateSprite(PieceType type)
        {
            Texture2D texture = CreateBaseTexture();
            Color[] pixels = texture.GetPixels();

            DrawShapeForPieceType(pixels, type);

            texture.SetPixels(pixels);
            texture.Apply();

            return CreateSpriteFromTexture(texture);
        }
        #endregion

        #region 비공개 메서드 - 텍스처 생성
        /// <summary>
        /// 투명한 기본 텍스처를 생성합니다.
        /// </summary>
        private Texture2D CreateBaseTexture()
        {
            Texture2D texture = new Texture2D(SPRITE_SIZE, SPRITE_SIZE);
            FillWithTransparent(texture);
            return texture;
        }

        /// <summary>
        /// 텍스처를투명하게 채웁니다.
        /// </summary>
        private void FillWithTransparent(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            texture.SetPixels(pixels);
        }

        /// <summary>
        /// 텍스처에서 스프라이트를 생성합니다.
        /// </summary>
        private Sprite CreateSpriteFromTexture(Texture2D texture)
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, SPRITE_SIZE, SPRITE_SIZE),
                new Vector2(SPRITE_PIVOT_X, SPRITE_PIVOT_Y),
                SPRITE_SIZE
            );
        }
        #endregion

        #region 비공개 메서드 - 도형 선택
        /// <summary>
        /// 기물 타입에 따라 적절한 도형을 그립니다.
        /// </summary>
        private void DrawShapeForPieceType(Color[] pixels, PieceType type)
        {
            Color drawColor = Color.white;

            switch (type)
            {
                case PieceType.King:
                    new CrossDrawer().Draw(pixels, SPRITE_SIZE, drawColor);
                    break;

                case PieceType.Queen:
                    new DiamondDrawer().Draw(pixels, SPRITE_SIZE, drawColor);
                    break;

                case PieceType.Rook:
                    new SquareDrawer().Draw(pixels, SPRITE_SIZE, drawColor);
                    break;

                case PieceType.Bishop:
                    new TriangleDrawer().Draw(pixels, SPRITE_SIZE, drawColor);
                    break;

                case PieceType.Knight:
                    new LShapeDrawer().Draw(pixels, SPRITE_SIZE, drawColor);
                    break;

                case PieceType.Pawn:
                    new CircleDrawer().Draw(pixels, SPRITE_SIZE, drawColor);
                    break;
            }
        }
        #endregion
    }

    /// <summary>
    /// 도형 그리기 인터페이스
    /// </summary>
    public interface IShapeDrawer
    {
        void Draw(Color[] pixels, int size, Color color);
    }

    /// <summary>원을 그립니다.</summary>
    public class CircleDrawer : IShapeDrawer
    {
        public void Draw(Color[] pixels, int size, Color color)
        {
            int center = size / 2;
            int radius = size / 3;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (IsInsideCircle(x, y, center, radius))
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }

        private bool IsInsideCircle(int x, int y, int center, int radius)
        {
            int dx = x - center;
            int dy = y - center;
            return dx * dx + dy * dy < radius * radius;
        }
    }

    /// <summary>사각형을 그립니다.</summary>
    public class SquareDrawer : IShapeDrawer
    {
        public void Draw(Color[] pixels, int size, Color color)
        {
            int margin = size / 4;

            for (int y = margin; y < size - margin; y++)
            {
                for (int x = margin; x < size - margin; x++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }
    }

    /// <summary>다이아몬드를 그립니다.</summary>
    public class DiamondDrawer : IShapeDrawer
    {
        public void Draw(Color[] pixels, int size, Color color)
        {
            int center = size / 2;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (IsInsideDiamond(x, y, center))
                    {
                        pixels[y * size + x] = color;
                    }
                }
            }
        }

        private bool IsInsideDiamond(int x, int y, int center)
        {
            int dx = Mathf.Abs(x - center);
            int dy = Mathf.Abs(y - center);
            return dx + dy < center;
        }
    }

    /// <summary>삼각형을 그립니다.</summary>
    public class TriangleDrawer : IShapeDrawer
    {
        public void Draw(Color[] pixels, int size, Color color)
        {
            int margin = size / 4;

            for (int y = margin; y < size - margin; y++)
            {
                DrawTriangleRow(pixels, size, y, margin, color);
            }
        }

        private void DrawTriangleRow(Color[] pixels, int size, int y, int margin, Color color)
        {
            int width = (y - margin) * 2 / 3;
            int center = size / 2;

            for (int x = center - width; x < center + width; x++)
            {
                if (x >= 0 && x < size)
                {
                    pixels[y * size + x] = color;
                }
            }
        }
    }

    /// <summary>십자가를 그립니다.</summary>
    public class CrossDrawer : IShapeDrawer
    {
        public void Draw(Color[] pixels, int size, Color color)
        {
            int center = size / 2;
            int thickness = size / 8;
            int margin = size / 4;

            DrawVerticalLine(pixels, size, center, thickness, margin, color);
            DrawHorizontalLine(pixels, size, center, thickness, margin, color);
        }

        private void DrawVerticalLine(Color[] pixels, int size, int center, int thickness, int margin, Color color)
        {
            for (int y = margin; y < size - margin; y++)
            {
                for (int x = center - thickness; x < center + thickness; x++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        private void DrawHorizontalLine(Color[] pixels, int size, int center, int thickness, int margin, Color color)
        {
            for (int x = margin; x < size - margin; x++)
            {
                for (int y = center - thickness; y < center + thickness; y++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }
    }

    /// <summary>L자 도형을 그립니다.</summary>
    public class LShapeDrawer : IShapeDrawer
    {
        public void Draw(Color[] pixels, int size, Color color)
        {
            int margin = size / 4;
            int thickness = size / 6;

            DrawVerticalPart(pixels, size, margin, thickness, color);
            DrawHorizontalPart(pixels, size, margin, thickness, color);
        }

        private void DrawVerticalPart(Color[] pixels, int size, int margin, int thickness, Color color)
        {
            for (int y = margin; y < size - margin; y++)
            {
                for (int x = margin; x < margin + thickness; x++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }

        private void DrawHorizontalPart(Color[] pixels, int size, int margin, int thickness, Color color)
        {
            for (int x = margin; x < size - margin; x++)
            {
                for (int y = size - margin - thickness; y < size - margin; y++)
                {
                    pixels[y * size + x] = color;
                }
            }
        }
    }
}

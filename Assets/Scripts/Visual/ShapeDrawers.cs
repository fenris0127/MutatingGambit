using UnityEngine;

namespace MutatingGambit.Visual
{
    /// <summary>
    /// 도형 그리기 인터페이스
    /// </summary>
    public interface IShapeDrawer
    {
        void Draw(Color[] pixels, int size, Color color);
    }

    #region Circle Drawer
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
    #endregion

    #region Square Drawer
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
    #endregion

    #region Diamond Drawer
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
    #endregion

    #region Triangle Drawer
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
    #endregion

    #region Cross Drawer
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
    #endregion

    #region L-Shape Drawer
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
    #endregion
}

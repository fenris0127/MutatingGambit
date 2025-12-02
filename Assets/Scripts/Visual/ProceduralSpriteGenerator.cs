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
}

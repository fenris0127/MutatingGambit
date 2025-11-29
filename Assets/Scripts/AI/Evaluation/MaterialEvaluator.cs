using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using System.Collections.Generic;

namespace MutatingGambit.AI.Evaluation
{
    /// <summary>
    /// 물질적 이점(기물 가치)을 평가합니다.
    /// </summary>
    public class MaterialEvaluator
    {
        private AIConfig config;
        private Team aiTeam;

        public MaterialEvaluator(AIConfig aiConfig, Team team)
        {
            config = aiConfig;
            aiTeam = team;
        }

        /// <summary>
        /// 물질적 이점을 평가합니다.
        /// </summary>
        public float EvaluateMaterial(Board board)
        {
            float aiMaterial = 0f;
            float opponentMaterial = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            var aiPieces = board.GetPiecesByTeam(aiTeam);
            var opponentPieces = board.GetPiecesByTeam(opponentTeam);

            foreach (var piece in aiPieces)
            {
                aiMaterial += GetPieceValue(piece);
            }

            foreach (var piece in opponentPieces)
            {
                opponentMaterial += GetPieceValue(piece);
            }

            return aiMaterial - opponentMaterial;
        }

        /// <summary>
        /// BoardState에서 물질적 이점을 평가합니다.
        /// </summary>
        public float EvaluateMaterialState(BoardState state)
        {
            float aiMaterial = 0f;
            float opponentMaterial = 0f;

            Team opponentTeam = aiTeam == Team.White ? Team.Black : Team.White;

            var aiPieces = state.GetPiecesByTeam(aiTeam);
            var opponentPieces = state.GetPiecesByTeam(opponentTeam);

            foreach (var piece in aiPieces)
            {
                aiMaterial += GetPieceValueFromType(piece.Type);
            }

            foreach (var piece in opponentPieces)
            {
                opponentMaterial += GetPieceValueFromType(piece.Type);
            }

            return aiMaterial - opponentMaterial;
        }

        private float GetPieceValue(Piece piece)
        {
            float baseValue = config.GetPieceValue(piece.Type);

            // 변이된 기물 보너스 (더 강력함)
            if (piece.HasMutations())
            {
                baseValue *= 1.2f;
            }

            return baseValue;
        }

        private float GetPieceValueFromType(PieceType type)
        {
            return config.GetPieceValue(type);
        }
    }
}

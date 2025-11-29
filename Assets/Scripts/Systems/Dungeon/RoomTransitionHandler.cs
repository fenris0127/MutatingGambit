using UnityEngine;
using MutatingGambit.Core.ChessEngine;
using MutatingGambit.Systems.PieceManagement;
using MutatingGambit.Systems.BoardSystem;

namespace MutatingGambit.Systems.Dungeon
{
    /// <summary>
    /// 방 진입 및 전환 로직을 처리합니다.
    /// </summary>
    public class RoomTransitionHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Board gameBoard;
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private RepairSystem repairSystem;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject piecePrefab;

        private DungeonManager dungeonManager;

        public void Initialize(DungeonManager manager)
        {
            dungeonManager = manager;
        }

        /// <summary>
        /// 전투 방에 진입합니다.
        /// </summary>
        public void EnterCombatRoom(RoomNode roomNode, PlayerState playerState)
        {
            RoomData roomData = roomNode.Room;

            // 보드 초기화
            InitializeBoard(roomData);

            // 플레이어 기물 복원
            if (playerState != null)
            {
                playerState.RestoreBoardState(gameBoard, repairSystem, piecePrefab);
            }

            // 적 기물 설정
            SetupEnemyPieces(roomData);

            // 아티팩트 적용
            ApplyArtifacts(playerState);

            // 룸 매니저 진입
            if (roomManager != null)
            {
                roomManager.SetBoard(gameBoard);
                roomManager.EnterRoom(roomData, playerState.PlayerTeam);
            }

            // 게임 시작
            if (gameManager != null)
            {
                gameManager.StartGame();
            }

            Debug.Log($"전투 방 진입: {roomData.RoomName}");
        }

        /// <summary>
        /// 보드를 초기화합니다.
        /// </summary>
        private void InitializeBoard(RoomData roomData)
        {
            if (gameBoard == null) return;

            gameBoard.Clear();

            if (roomData.BoardData != null)
            {
                gameBoard.Initialize(roomData.BoardData.Width, roomData.BoardData.Height);

                // 장애물 배치
                for (int y = 0; y < roomData.BoardData.Height; y++)
                {
                    for (int x = 0; x < roomData.BoardData.Width; x++)
                    {
                        Vector2Int pos = new Vector2Int(x, y);
                        if (roomData.BoardData.GetTileType(pos) == TileType.Obstacle)
                        {
                            if (gameBoard.IsPositionValid(pos))
                            {
                                gameBoard.SetObstacle(pos, true);
                            }
                        }
                    }
                }
            }
            else
            {
                gameBoard.Initialize(8, 8);
            }
        }

        /// <summary>
        /// 적 기물을 설정합니다.
        /// </summary>
        private void SetupEnemyPieces(RoomData roomData)
        {
            if (roomData.EnemyPieces == null || roomData.EnemyPieces.Length == 0)
            {
                Debug.LogWarning($"방 '{roomData.RoomName}'에 적 기물이 없습니다");
                return;
            }

            foreach (var pieceData in roomData.EnemyPieces)
            {
                Piece piece = CreatePiece(pieceData.pieceType, pieceData.team, pieceData.position);
                if (piece != null && gameBoard != null)
                {
                    gameBoard.PlacePiece(piece, pieceData.position);
                }
            }

            // 적 변이 적용
            if (roomData.EnemyMutations != null)
            {
                foreach (var mutationData in roomData.EnemyMutations)
                {
                    if (mutationData.mutation != null && gameBoard != null)
                    {
                        var piece = gameBoard.GetPiece(mutationData.piecePosition);
                        if (piece != null)
                        {
                            mutationData.mutation.ApplyToPiece(piece);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 기물을 생성합니다.
        /// </summary>
        private Piece CreatePiece(PieceType type, Team team, Vector2Int position)
        {
            GameObject pieceObject = piecePrefab != null 
                ? Instantiate(piecePrefab) 
                : new GameObject($"{team}_{type}");

            pieceObject.name = $"{team}_{type}";

            Piece piece = pieceObject.GetComponent<Piece>();
            if (piece == null)
            {
                piece = pieceObject.AddComponent<Piece>();
            }

            piece.Initialize(type, team, position);
            return piece;
        }

        /// <summary>
        /// 아티팩트를 적용합니다.
        /// </summary>
        private void ApplyArtifacts(PlayerState playerState)
        {
            if (playerState?.CollectedArtifacts == null || gameBoard?.ArtifactManager == null)
            {
                return;
            }

            foreach (var artifact in playerState.CollectedArtifacts)
            {
                if (artifact != null)
                {
                    gameBoard.ArtifactManager.AddArtifact(artifact);
                }
            }

            Debug.Log($"{playerState.CollectedArtifacts.Count}개 아티팩트 적용");
        }
    }
}

using UnityEngine;
using UnityEditor;
using MutatingGambit.Systems.Mutations;

namespace MutatingGambit.Editor
{
    /// <summary>
    /// Mutation ScriptableObject 에셋을 생성합니다.
    /// 단일 책임: Mutation 타입 에셋 생성만 담당
    /// </summary>
    public class MutationAssetCreator
    {
        #region 상수
        private readonly string folderPath;
        private readonly AssetCreationHelper helper;
        #endregion

        #region 생성자
        /// <summary>
        /// Mutation 에셋 생성기를 초기화합니다.
        /// </summary>
        /// <param name="path">에셋을 생성할 폴더 경로</param>
        /// <param name="creationHelper">에셋 생성 도우미</param>
        public MutationAssetCreator(string path, AssetCreationHelper creationHelper)
        {
            folderPath = path;
            helper = creationHelper;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 모든 Mutation 에셋을 생성합니다.
        /// </summary>
        /// <returns>생성된 에셋 수</returns>
        public int CreateAll()
        {
            Debug.Log("Mutation 에셋 생성 중...");

            CreateLeapingRook();
            CreateSplittingKnight();
            CreateFragileBishop();
            CreatePhantomPawn();
            CreateBerserkQueen();
            CreateTeleportingKnight();
            CreateFrozenBishop();
            CreateDoubleMoveRook();
            CreateExplosiveKing();
            CreateShadowPawn();

            return 10;
        }
        #endregion

        #region 비공개 메서드 - 개별 Mutation 생성
        private void CreateLeapingRook()
        {
            helper.CreateAsset<LeapingRookMutation>(folderPath, "LeapingRook");
        }

        private void CreateSplittingKnight()
        {
            helper.CreateAsset<SplittingKnightMutation>(folderPath, "SplittingKnight");
        }

        private void CreateFragileBishop()
        {
            helper.CreateAsset<FragileBishopMutation>(folderPath, "FragileBishop");
        }

        private void CreatePhantomPawn()
        {
            helper.CreateAsset<PhantomPawnMutation>(folderPath, "PhantomPawn");
        }

        private void CreateBerserkQueen()
        {
            helper.CreateAsset<BerserkQueenMutation>(folderPath, "BerserkQueen");
        }

        private void CreateTeleportingKnight()
        {
            helper.CreateAsset<TeleportingKnightMutation>(folderPath, "TeleportingKnight");
        }

        private void CreateFrozenBishop()
        {
            helper.CreateAsset<FrozenBishopMutation>(folderPath, "FrozenBishop");
        }

        private void CreateDoubleMoveRook()
        {
            helper.CreateAsset<DoubleMoveRookMutation>(folderPath, "DoubleMoveRook");
        }

        private void CreateExplosiveKing()
        {
            helper.CreateAsset<ExplosiveKingMutation>(folderPath, "ExplosiveKing");
        }

        private void CreateShadowPawn()
        {
            helper.CreateAsset<ShadowPawnMutation>(folderPath, "ShadowPawn");
        }
        #endregion
    }

    /// <summary>
    /// ScriptableObject 에셋 생성을 돕는 유틸리티 클래스입니다.
    /// </summary>
    public class AssetCreationHelper
    {
        #region 공개 메서드
        /// <summary>
        /// ScriptableObject 에셋을 생성합니다.
        /// </summary>
        /// <typeparam name="T">생성할 에셋 타입</typeparam>
        /// <param name="folderPath">폴더 경로</param>
        /// <param name="assetName">에셋 이름</param>
        /// <returns>생성된 또는 기존 에셋</returns>
        public T CreateAsset<T>(string folderPath, string assetName) where T : ScriptableObject
        {
            string assetPath = BuildAssetPath(folderPath, assetName);

            T existingAsset = TryLoadExistingAsset<T>(assetPath);
            if (existingAsset != null)
            {
                return existingAsset;
            }

            return CreateNewAsset<T>(assetPath);
        }
        #endregion

        #region 비공개 메서드
        /// <summary>
        /// 에셋 경로를 생성합니다.
        /// </summary>
        private string BuildAssetPath(string folder, string name)
        {
            return $"{folder}/{name}.asset";
        }

        /// <summary>
        /// 기존 에셋을 로드 시도합니다.
        /// </summary>
        private T TryLoadExistingAsset<T>(string path) where T : ScriptableObject
        {
            T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            
            if (existingAsset != null)
            {
                LogAssetExists(path);
            }
            
            return existingAsset;
        }

        /// <summary>
        /// 새 에셋을 생성합니다.
        /// </summary>
        private T CreateNewAsset<T>(string path) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            LogAssetCreated(path);
            return asset;
        }

        /// <summary>
        /// 에셋 존재 로그를 출력합니다.
        /// </summary>
        private void LogAssetExists(string path)
        {
            Debug.Log($"에셋이 이미 존재합니다: {path}");
        }

        /// <summary>
        /// 에셋 생성 로그를 출력합니다.
        /// </summary>
        private void LogAssetCreated(string path)
        {
            Debug.Log($"에셋 생성 완료: {path}");
        }
        #endregion
    }
}

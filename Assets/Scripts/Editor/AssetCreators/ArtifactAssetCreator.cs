using UnityEngine;
using UnityEditor;
using MutatingGambit.Systems.Artifacts;

namespace MutatingGambit.Editor
{
    /// <summary>
    /// Artifact ScriptableObject 에셋을 생성합니다.
    /// 단일 책임: Artifact 타입 에셋 생성만 담당
    /// </summary>
    public class ArtifactAssetCreator
    {
        #region 상수
        private readonly string folderPath;
        private readonly AssetCreationHelper helper;
        #endregion

        #region 생성자
        public ArtifactAssetCreator(string path, AssetCreationHelper creationHelper)
        {
            folderPath = path;
            helper = creationHelper;
        }
        #endregion

        #region 공개 메서드
        /// <summary>
        /// 모든 Artifact 에셋을 생성합니다.
        /// </summary>
        /// <returns>생성된 에셋 수</returns>
        public int CreateAll()
        {
            Debug.Log("Artifact 에셋 생성 중...");

            CreateExistingArtifacts();
            CreateNewArtifacts();

            return 21; // 7개 기존 + 14개 신규
        }
        #endregion

        #region 비공개 메서드 - 에셋 그룹 생성
        /// <summary>
        /// 기존 Artifact들을 생성합니다.
        /// </summary>
        private void CreateExistingArtifacts()
        {
            helper.CreateAsset<GravityMirrorArtifact>(folderPath, "GravityMirror");
            helper.CreateAsset<KingsShadowArtifact>(folderPath, "KingsShadow");
            helper.CreateAsset<CavalryChargeArtifact>(folderPath, "CavalryCharge");
            helper.CreateAsset<TimeWarpArtifact>(folderPath, "TimeWarp");
            helper.CreateAsset<MagneticFieldArtifact>(folderPath, "MagneticField");
            helper.CreateAsset<PhoenixFeatherArtifact>(folderPath, "PhoenixFeather");
            helper.CreateAsset<BloodMoonArtifact>(folderPath, "BloodMoon");
        }

        /// <summary>
        /// 신규 Artifact들을 생성합니다.
        /// </summary>
        private void CreateNewArtifacts()
        {
            CreateNewArtifactsGroup1();
            CreateNewArtifactsGroup2();
        }

        /// <summary>
        /// 신규 Artifact 그룹 1을 생성합니다.
        /// </summary>
        private void CreateNewArtifactsGroup1()
        {
            helper.CreateAsset<ChainLightningArtifact>(folderPath, "ChainLightning");
            helper.CreateAsset<PromotionPrivilegeArtifact>(folderPath, "PromotionPrivilege");
            helper.CreateAsset<FrozenThroneArtifact>(folderPath, "FrozenThrone");
            helper.CreateAsset<MimicsMaskArtifact>(folderPath, "MimicsMask");
            helper.CreateAsset<BerserkersRageArtifact>(folderPath, "BerserkersRage");
            helper.CreateAsset<SanctuaryShieldArtifact>(folderPath, "SanctuaryShield");
            helper.CreateAsset<PhantomStepsArtifact>(folderPath, "PhantomSteps");
        }

        /// <summary>
        /// 신규 Artifact 그룹 2를 생성합니다.
        /// </summary>
        private void CreateNewArtifactsGroup2()
        {
            helper.CreateAsset<TwinSoulsArtifact>(folderPath, "TwinSouls");
            helper.CreateAsset<CursedCrownArtifact>(folderPath, "CursedCrown");
            helper.CreateAsset<DivineInterventionArtifact>(folderPath, "DivineIntervention");
            helper.CreateAsset<HasteBootsArtifact>(folderPath, "HasteBoots");
            helper.CreateAsset<SacrificialAltarArtifact>(folderPath, "SacrificialAltar");
            helper.CreateAsset<WeakeningAuraArtifact>(folderPath, "WeakeningAura");
            helper.CreateAsset<ResurrectionStoneArtifact>(folderPath, "ResurrectionStone");
        }
        #endregion
    }
}

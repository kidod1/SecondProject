using Spine.Unity;
using UnityEngine;

public class SkeletonReloader : MonoBehaviour
{
    [SerializeField] private SkeletonGraphic skeletonGraphic;
    [SerializeField] private SkeletonDataAsset skeletonDataAsset;

    public void ForceReloadSkeletonData()
    {
        if (skeletonDataAsset != null)
        {
            // 캐시된 데이터를 삭제하고 다시 로드
            skeletonDataAsset.Clear();
            skeletonDataAsset.GetSkeletonData(true); // 다시 로드

            // SkeletonGraphic에 새로 로드된 데이터 적용
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Skeleton.SetToSetupPose(); // 기본 포즈로 설정
            Debug.Log("SkeletonDataAsset이 리로드되었습니다.");
        }
    }
}

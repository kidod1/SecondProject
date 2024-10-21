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
            // ĳ�õ� �����͸� �����ϰ� �ٽ� �ε�
            skeletonDataAsset.Clear();
            skeletonDataAsset.GetSkeletonData(true); // �ٽ� �ε�

            // SkeletonGraphic�� ���� �ε�� ������ ����
            skeletonGraphic.Initialize(true);
            skeletonGraphic.Skeleton.SetToSetupPose(); // �⺻ ����� ����
            Debug.Log("SkeletonDataAsset�� ���ε�Ǿ����ϴ�.");
        }
    }
}

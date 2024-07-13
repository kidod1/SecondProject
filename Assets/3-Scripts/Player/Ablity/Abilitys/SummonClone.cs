using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SummonClone")]
public class SummonClone : Ability
{
    public GameObject clonePrefab; // 분신체 프리팹
    private GameObject cloneInstance; // 분신체 인스턴스
    private RotatingObject rotatingObject;
    private float[] damageMultipliers = { 0.3f, 0.5f, 0.7f, 1.0f, 1.2f }; // 레벨별 피해 배율

    public override void Apply(Player player)
    {
        if (currentLevel == 0)
        {
            // 분신체를 소환하고 Player의 자식으로 설정
            cloneInstance = Instantiate(clonePrefab, player.transform);
            rotatingObject = cloneInstance.GetComponent<RotatingObject>();
            if (rotatingObject != null)
            {
                rotatingObject.player = player.transform;
                rotatingObject.playerShooting = player;
                rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
            }
            else
            {
                Debug.LogError("RotatingObject component is missing in the clonePrefab.");
            }
        }
        else
        {
            // 기존 분신체의 피해 배율을 업데이트
            if (rotatingObject != null)
            {
                rotatingObject.damageMultiplier = damageMultipliers[currentLevel];
            }
            else
            {
                Debug.LogError("RotatingObject is not initialized.");
            }
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        if (cloneInstance != null)
        {
            Destroy(cloneInstance);
            cloneInstance = null;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return (int)(damageMultipliers[currentLevel] * 100);
        }
        return 0;
    }
}

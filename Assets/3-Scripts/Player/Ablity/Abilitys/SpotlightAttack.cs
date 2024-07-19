using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SpotlightAttack")]
public class SpotlightAttack : Ability
{
    public GameObject spotlightPrefab; // 스포트라이트 프리팹
    private SpotlightEffect spotlightInstance; // 스포트라이트 인스턴스

    private int[] damageValues = { 3, 5, 7, 10, 15 };
    private float[] rangeValues = { 2f, 2.5f, 2.5f, 2.5f, 3f };
    private float[] intervalValues = { 1f, 1f, 1f, 0.75f, 0.75f };

    public override void Apply(Player player)
    {
        if (currentLevel == 0)
        {
            // 스포트라이트를 소환하고 Player의 자식으로 설정
            GameObject spotlightObject = Instantiate(spotlightPrefab, player.transform);
            spotlightInstance = spotlightObject.GetComponent<SpotlightEffect>();
            if (spotlightInstance != null)
            {
                spotlightInstance.player = player;
                spotlightInstance.damageAmount = damageValues[currentLevel];
                spotlightInstance.damageRadius = rangeValues[currentLevel];
                spotlightInstance.damageInterval = intervalValues[currentLevel];
                spotlightInstance.currentLevel = currentLevel + 1; // 현재 레벨 설정
            }
            else
            {
                Debug.LogError("SpotlightEffect component is missing in the spotlightPrefab.");
            }
        }
        else
        {
            // 기존 스포트라이트의 값을 업데이트
            if (spotlightInstance != null)
            {
                spotlightInstance.damageAmount = damageValues[currentLevel];
                spotlightInstance.damageRadius = rangeValues[currentLevel];
                spotlightInstance.damageInterval = intervalValues[currentLevel];
                spotlightInstance.currentLevel = currentLevel + 1; // 현재 레벨 설정
            }
            else
            {
                Debug.LogError("SpotlightEffect is not initialized.");
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
        if (spotlightInstance != null)
        {
            Destroy(spotlightInstance.gameObject);
            spotlightInstance = null;
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return damageValues[currentLevel];
        }
        return 0;
    }
}

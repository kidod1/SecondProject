using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SpotlightAttack")]
public class SpotlightAttack : Ability
{
    public GameObject spotlightPrefab; // ����Ʈ����Ʈ ������
    private SpotlightEffect spotlightInstance; // ����Ʈ����Ʈ �ν��Ͻ�

    private int[] damageValues = { 3, 5, 7, 10, 15 };
    private float[] rangeValues = { 2f, 2.5f, 2.5f, 2.5f, 3f };
    private float[] intervalValues = { 1f, 1f, 1f, 0.75f, 0.75f };

    public override void Apply(Player player)
    {
        if (currentLevel == 0)
        {
            // ����Ʈ����Ʈ�� ��ȯ�ϰ� Player�� �ڽ����� ����
            GameObject spotlightObject = Instantiate(spotlightPrefab, player.transform);
            spotlightInstance = spotlightObject.GetComponent<SpotlightEffect>();
            if (spotlightInstance != null)
            {
                spotlightInstance.player = player;
                spotlightInstance.damageAmount = damageValues[currentLevel];
                spotlightInstance.damageRadius = rangeValues[currentLevel];
                spotlightInstance.damageInterval = intervalValues[currentLevel];
                spotlightInstance.currentLevel = currentLevel + 1; // ���� ���� ����
            }
            else
            {
                Debug.LogError("SpotlightEffect component is missing in the spotlightPrefab.");
            }
        }
        else
        {
            // ���� ����Ʈ����Ʈ�� ���� ������Ʈ
            if (spotlightInstance != null)
            {
                spotlightInstance.damageAmount = damageValues[currentLevel];
                spotlightInstance.damageRadius = rangeValues[currentLevel];
                spotlightInstance.damageInterval = intervalValues[currentLevel];
                spotlightInstance.currentLevel = currentLevel + 1; // ���� ���� ����
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

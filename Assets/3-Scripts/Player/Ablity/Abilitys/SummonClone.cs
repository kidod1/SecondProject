using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SummonClone")]
public class SummonClone : Ability
{
    public GameObject clonePrefab; // �н�ü ������
    private GameObject cloneInstance; // �н�ü �ν��Ͻ�
    private RotatingObject rotatingObject;
    private float[] damageMultipliers = { 0.3f, 0.5f, 0.7f, 1.0f, 1.2f }; // ������ ���� ����

    public override void Apply(Player player)
    {
        if (currentLevel == 0)
        {
            // �н�ü�� ��ȯ�ϰ� Player�� �ڽ����� ����
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
            // ���� �н�ü�� ���� ������ ������Ʈ
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

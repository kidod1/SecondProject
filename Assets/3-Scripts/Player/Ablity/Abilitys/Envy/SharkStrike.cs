using System.Collections;
using UnityEngine;
using AK.Wwise; // WWISE ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "Abilities/SharkStrike")]
public class SharkStrike : Ability
{
    [Tooltip("������ ��� ������ ������")]
    public int[] damageIncreases = { 10, 20, 30, 40, 50 };  // ���� 1~5

    public GameObject sharkPrefab;  // ��� ������
    public int hitThreshold = 5;  // ���� �Ӱ谪
    public float sharkSpeed = 5f;  // ��� �ӵ�
    public float chaseDelay = 0.5f;  // ��� �߰� ���� �� ��� �ð�
    public float maxSearchTime = 3f; // �� ���͸� ã�� �ִ� �ð�

    private Player playerInstance;
    private int hitCount = 0;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;

        if (hitCount >= hitThreshold)
        {
            SpawnShark();
            hitCount = 0;
        }
    }

    private void SpawnShark()
    {
        if (sharkPrefab != null && playerInstance != null)
        {
            GameObject sharkObject = Instantiate(sharkPrefab, playerInstance.transform.position, Quaternion.identity);
            Shark sharkInstance = sharkObject.GetComponent<Shark>();

            if (sharkInstance != null)
            {
                int damageIncrease = GetSharkDamageIncrease();
                sharkInstance.Initialize(sharkSpeed, chaseDelay, maxSearchTime, damageIncrease);

            }
            else
            {
                Debug.LogError("Shark ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("SharkPrefab �Ǵ� PlayerInstance�� �������� �ʾҽ��ϴ�.");
        }
    }

    private int GetSharkDamageIncrease()
    {
        if (currentLevel < damageIncreases.Length)
        {
            return damageIncreases[currentLevel - 1];
        }
        else
        {
            return damageIncreases[damageIncreases.Length - 1];
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            // ���׷��̵� ���� �߰� �ʿ� �� ����
        }
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < damageIncreases.Length)
        {
            return damageIncreases[currentLevel + 1];
        }
        return 0;
    }

    public override void ResetLevel()
    {
        hitCount = 0;
        currentLevel = 0;
    }

    public override string GetDescription()
    {
        if (currentLevel < damageIncreases.Length && currentLevel >= 0)
        {
            int damageIncrease = damageIncreases[currentLevel];
            return $"{baseDescription}\nLv {currentLevel + 1}: ���� {hitThreshold}ȸ ���� ������ ���� ����ٴϴ� ��� ��ȯ. ������ +{damageIncrease}";
        }
        else if (currentLevel >= damageIncreases.Length)
        {
            int maxDamageIncrease = damageIncreases[damageIncreases.Length - 1];
            return $"{baseDescription}\n�ִ� ���� ����: ���� {hitThreshold}ȸ ���� ������ ���� ����ٴϴ� ��� ��ȯ. ������ +{maxDamageIncrease}";
        }
        else
        {
            return $"{baseDescription}\n�ִ� ���� ����.";
        }
    }
}

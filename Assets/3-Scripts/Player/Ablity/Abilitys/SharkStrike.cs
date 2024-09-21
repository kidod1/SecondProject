using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/SharkStrike")]
public class SharkStrike : Ability
{
    public GameObject sharkPrefab;  // ��� ������
    public int hitThreshold = 5;  // ���� �Ӱ谪
    public float sharkSpeed = 5f;  // ��� �ӵ�
    public float chaseDelay = 0.5f;  // ��� �߰� ���� �� ��� �ð�

    private Player playerInstance;
    private int hitCount = 0;

    public override void Apply(Player player)
    {
        playerInstance = player;
    }

    // �÷��̾ ���� ���߽����� �� ȣ��Ǵ� �޼���
    public void OnProjectileHit(Collider2D enemy)
    {
        hitCount++;

        if (hitCount >= hitThreshold)
        {
            SpawnShark();
            hitCount = 0;
        }
    }

    // �� �����ϴ� �޼���
    private void SpawnShark()
    {
        if (sharkPrefab != null)
        {
            GameObject sharkObject = Instantiate(sharkPrefab, playerInstance.transform.position, Quaternion.identity);
            Shark sharkInstance = sharkObject.GetComponent<Shark>();

            if (sharkInstance != null)
            {
                sharkInstance.Initialize(sharkSpeed, chaseDelay);
            }
            else
            {
                Debug.LogError("Shark component is missing from the prefab.");
            }
        }
        else
        {
            Debug.LogError("Shark prefab is null. Cannot spawn shark.");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        hitCount = 0;  // ���� Ƚ�� �ʱ�ȭ
    }

    protected override int GetNextLevelIncrease()
    {
        return 1;  // ���� ���� �� ���� ����
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
            // ������ ���� ��� �ӵ� �Ǵ� ���� �Ӱ谪�� ������ �� ����
        }
    }
}

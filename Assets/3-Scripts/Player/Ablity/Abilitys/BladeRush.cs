using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Abilities/BladeRush")]
public class BladeRush : Ability
{
    [Header("Ability Parameters")]
    [Tooltip("�ɷ� �ߵ� ��Ÿ�� (��)")]
    public float cooldown = 5f;

    [Tooltip("Į���� ���ط�")]
    public int damage = 20;

    [Tooltip("Į���� ��Ÿ�")]
    public float range = 10f;

    [Tooltip("Į���� �̵� �ӵ�")]
    public float bladeSpeed = 15f;

    [Tooltip("Į���� ������")]
    public GameObject bladePrefab;

    private Player playerInstance;
    private Coroutine abilityCoroutine;

    public override void Apply(Player player)
    {
        if (player == null)
        {
            Debug.LogError("BladeRush Apply: player �ν��Ͻ��� null�Դϴ�.");
            return;
        }

        playerInstance = player;

        if (abilityCoroutine == null)
        {
            abilityCoroutine = player.StartCoroutine(AbilityCoroutine());
        }
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel - 1)
        {
            currentLevel++;
            damage += 10;
            range += 2f;
        }
        else
        {
            Debug.LogWarning("BladeRush: �̹� �ִ� ������ �����߽��ϴ�.");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        if (abilityCoroutine != null)
        {
            playerInstance.StopCoroutine(abilityCoroutine);
            abilityCoroutine = null;
        }

        currentLevel = 0;
        damage = 20;
        range = 10f;
    }

    private IEnumerator AbilityCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldown);
            FireBlade();
        }
    }

    private void FireBlade()
    {
        if (bladePrefab == null)
        {
            Debug.LogError("BladeRush: bladePrefab�� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }

        Vector3 spawnPosition = playerInstance.transform.position;

        // �÷��̾��� �ٶ󺸴� ������ �����ɴϴ�.
        Vector2 facingDirection = playerInstance.GetFacingDirection();

        // Į���� ȸ���� ���⿡ �°� �����մϴ�.
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x) * Mathf.Rad2Deg;
        Quaternion spawnRotation = Quaternion.Euler(0f, 0f, angle - 90);

        GameObject bladeInstance = Instantiate(bladePrefab, spawnPosition, spawnRotation);
        BladeProjectile bladeProjectile = bladeInstance.GetComponent<BladeProjectile>();

        if (bladeProjectile != null)
        {
            bladeProjectile.Initialize(damage, range, bladeSpeed, playerInstance, facingDirection);
        }
        else
        {
            Debug.LogError("BladeRush: bladePrefab�� BladeProjectile ������Ʈ�� �����ϴ�.");
        }
    }

    public override string GetDescription()
    {
        string description = $"{baseDescription}\n";
        description += $"��Ÿ��: {cooldown}��\n";
        description += $"���ط�: {damage}\n";
        description += $"��Ÿ�: {range}";

        return description;
    }

    // GetNextLevelIncrease �޼��� ����
    protected override int GetNextLevelIncrease()
    {
        if (currentLevel + 1 < maxLevel)
        {
            int nextDamageIncrease = 10;  // ���� �������� �߰��� ���ط�
            return nextDamageIncrease;
        }

        return 0;
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ReverseAttack")]
public class ReverseAttack : Ability
{
    public int[] attackIncreases;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        // �ߺ� ��� ����
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShoot);
        }

        playerInstance = player;
        player.OnShoot.AddListener(OnShoot);

        if (currentLevel == 1)
        {
            Debug.Log("���� ���� Lv 1");
        }
        else if (currentLevel > 1)
        {
            player.stat.playerDamage += attackIncreases[currentLevel - 1];
        }

        Debug.Log($"���� ������ ����Ǿ����ϴ�. ���� ���� Lv: {currentLevel}");
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }

        Debug.Log($"���� ���� ���׷��̵�. ���� ���� : {currentLevel}");
    }

    protected override int GetNextLevelIncrease()
    {
        if (currentLevel < maxLevel)
        {
            return attackIncreases[currentLevel];
        }
        return 0;
    }

    private void ShootReverse(Player player, Vector2 direction, int prefabIndex)
    {
        GameObject projectile = player.objectPool.GetObject(prefabIndex);
        projectile.transform.position = player.transform.position;

        Projectile projScript = projectile.GetComponent<Projectile>();
        if (projScript != null)
        {
            projScript.Initialize(player.stat);
            projScript.SetDirection(direction);
            Debug.Log($"�ݴ� ������ ����. ��ĩ�� : {direction}");
        }
        else
        {
            Debug.LogError("������Ÿ�� ��ũ��Ʈ�� �����ϴ�..");
        }
    }

    private void OnShoot(Vector2 direction, int prefabIndex)
    {
        if (playerInstance != null)
        {
            Vector2 reverseDirection = -direction;
            ShootReverse(playerInstance, reverseDirection, prefabIndex);
            Debug.Log($"�ݴ� ������ ����. ��ĩ��: {direction}, �ݴ� ������ ��ĩ�� : {reverseDirection}");
        }
    }

    public override void ResetLevel()
    {
        base.ResetLevel();
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShoot);
        }
        playerInstance = null;
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/HomingAttack")]
public class HomingAttack : Ability
{
    public float homingStartDelay = 0.3f;
    public float homingSpeed = 5f;
    public float homingRange = 10f;  // ���� ����

    private Player playerInstance;

    public override void Apply(Player player)
    {
        playerInstance = player;

        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShoot);
        }

        player.stat.currentProjectileType = 1;

        playerInstance.OnShoot.AddListener(OnShoot);
        Debug.Log($"���� ������ ����Ǿ����ϴ�. ���� ���� Lv: {currentLevel}");
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        // �̺�Ʈ ������ ����
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShoot);
        }
        playerInstance = null;
    }


    private void OnShoot(Vector2 direction, int prefabIndex)
    {
        // ����ź ����
        GameObject projectile = playerInstance.objectPool.GetObject(prefabIndex);
        if (projectile == null)
        {
            Debug.LogError("Projectile could not be instantiated.");
            return;
        }

        // ��ġ ������ shootPoint�� �ٽ� Ȯ��
        projectile.transform.position = playerInstance.shootPoint.position;

        HomingProjectile projScript = projectile.GetComponent<HomingProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(playerInstance.stat, homingStartDelay, homingSpeed, homingRange);
            projScript.SetDirection(direction);
        }
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
        return 0;
    }
}

using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ReverseAttack")]
public class ReverseAttack : Ability
{
    public int[] attackIncreases;

    private Player playerInstance; // �̺�Ʈ �����ʸ� �����ϱ� ���� Player �ν��Ͻ��� ����

    public override void Apply(Player player)
    {
        playerInstance = player;

        // ������ ������� �׻� �̺�Ʈ �����ʸ� �߰�
        player.OnShoot.AddListener(OnShoot);

        if (currentLevel == 1)
        {
            Debug.Log("ReverseAttack Level 1 Applied");
        }
        else if (currentLevel > 1)
        {
            // ���� 2 �̻�: �÷��̾��� ���� ����
            player.stat.playerDamage += attackIncreases[currentLevel - 1];
        }

        Debug.Log($"ReverseAttack applied. Current level: {currentLevel}");
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }

        Debug.Log($"ReverseAttack upgraded. New level: {currentLevel}");
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
        projectile.GetComponent<Projectile>().SetDirection(direction);
        Debug.Log($"ShootReverse called with direction: {direction}");
    }

    private void OnShoot(Vector2 direction, int prefabIndex)
    {
        if (playerInstance != null)
        {
            Vector2 reverseDirection = -direction;
            ShootReverse(playerInstance, reverseDirection, prefabIndex);
            Debug.Log($"OnShoot called with direction: {direction}, reverse direction: {reverseDirection}");
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

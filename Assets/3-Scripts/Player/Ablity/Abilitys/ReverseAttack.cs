using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/ReverseAttack")]
public class ReverseAttack : Ability
{
    public int[] attackIncreases;

    private Player playerInstance;

    public override void Apply(Player player)
    {
        // 중복 등록 방지
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShoot);
        }

        playerInstance = player;
        player.OnShoot.AddListener(OnShoot);

        if (currentLevel == 1)
        {
            Debug.Log("반전 공격 Lv 1");
        }
        else if (currentLevel > 1)
        {
            player.stat.playerDamage += attackIncreases[currentLevel - 1];
        }

        Debug.Log($"반전 공격이 적용되었습니다. 현재 레벨 Lv: {currentLevel}");
    }

    public override void Upgrade()
    {
        if (currentLevel < maxLevel)
        {
            currentLevel++;
        }

        Debug.Log($"반전 공격 업그레이드. 현재 레벨 : {currentLevel}");
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
            Debug.Log($"반대 방향을 지정. 위칫값 : {direction}");
        }
        else
        {
            Debug.LogError("프로젝타일 스크립트가 없습니다..");
        }
    }

    private void OnShoot(Vector2 direction, int prefabIndex)
    {
        if (playerInstance != null)
        {
            Vector2 reverseDirection = -direction;
            ShootReverse(playerInstance, reverseDirection, prefabIndex);
            Debug.Log($"반대 방향을 지정. 위칫값: {direction}, 반대 공격의 위칫값 : {reverseDirection}");
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

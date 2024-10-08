using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/HomingAttack")]
public class HomingAttack : Ability
{
    public float homingStartDelay = 0.3f;
    public float homingSpeed = 5f;
    public float homingRange = 10f;  // 유도 범위

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
        Debug.Log($"유도 공격이 적용되었습니다. 현재 레벨 Lv: {currentLevel}");
    }

    public override void ResetLevel()
    {
        base.ResetLevel();

        // 이벤트 리스너 제거
        if (playerInstance != null)
        {
            playerInstance.OnShoot.RemoveListener(OnShoot);
        }
        playerInstance = null;
    }


    private void OnShoot(Vector2 direction, int prefabIndex)
    {
        // 유도탄 생성
        GameObject projectile = playerInstance.objectPool.GetObject(prefabIndex);
        if (projectile == null)
        {
            Debug.LogError("Projectile could not be instantiated.");
            return;
        }

        // 위치 설정을 shootPoint로 다시 확인
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

        Debug.Log($"유도 공격 업그레이드. 현재 레벨 : {currentLevel}");
    }

    protected override int GetNextLevelIncrease()
    {
        return 0;
    }
}

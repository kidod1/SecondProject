using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ActiveAbilities/CoinRain")]
public class CoinRain : SynergyAbility
{
    public GameObject coinRainPrefab;
    public float coinRainDuration = 5f;
    public float damageInterval = 0.5f;
    public int damageAmount = 20;
    public float coinAbilityCooldownDurations = 8f;

    private Player playerInstance;
    private GameObject activeCoinRain;
    private Camera mainCamera;
    private ParticleSystem particleSystem;

    private void OnEnable()
    {
        cooldownDuration = coinAbilityCooldownDurations;
    }

    public override void Apply(Player player)
    {
        base.Apply(player);
        playerInstance = player;
        mainCamera = Camera.main;

        if (activeCoinRain == null)
        {
            Vector3 spawnPosition = mainCamera.transform.position;
            activeCoinRain = Instantiate(coinRainPrefab, spawnPosition, Quaternion.identity);

            particleSystem = activeCoinRain.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                particleSystem.Play();
            }

            playerInstance.StartCoroutine(DamageMonstersInCamera());
        }
    }

    private IEnumerator DamageMonstersInCamera()
    {
        float elapsedTime = 0f;

        while (elapsedTime < coinRainDuration)
        {
            DealDamageToVisibleMonsters();
            elapsedTime += damageInterval;
            yield return new WaitForSeconds(damageInterval);
        }

        DeactivateCoinRain();
    }

    private void DealDamageToVisibleMonsters()
    {
        Collider2D[] monstersInCamera = GetMonstersInCamera();
        foreach (Collider2D monsterCollider in monstersInCamera)
        {
            Monster monster = monsterCollider.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damageAmount, PlayManager.I.GetPlayerPosition());
            }
        }
    }

    private Collider2D[] GetMonstersInCamera()
    {
        Vector3 cameraBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 cameraTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        Vector2 cameraSize = new Vector2(cameraTopRight.x - cameraBottomLeft.x, cameraTopRight.y - cameraBottomLeft.y);
        Vector2 cameraCenter = new Vector2(mainCamera.transform.position.x, mainCamera.transform.position.y);

        return Physics2D.OverlapBoxAll(cameraCenter, cameraSize, 0, LayerMask.GetMask("Monster"));
    }

    private void DeactivateCoinRain()
    {
        if (activeCoinRain != null)
        {
            if (particleSystem != null)
            {
                particleSystem.Stop();
            }

            Destroy(activeCoinRain);
            activeCoinRain = null;
        }
    }

    public override void Upgrade()
    {
    }
    public override void ResetLevel()
    {
        base.ResetLevel();
        lastUsedTime = 0;
    }
}

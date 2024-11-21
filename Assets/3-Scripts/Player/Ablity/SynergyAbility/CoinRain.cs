using System.Collections;
using UnityEngine;
using AK.Wwise; // Wwise 네임스페이스 추가

[CreateAssetMenu(menuName = "ActiveAbilities/CoinRain")]
public class CoinRain : SynergyAbility
{
    [Header("코인비 파라미터")]
    [InspectorName("코인 레인 프리팹")]
    public GameObject coinRainPrefab;

    [InspectorName("코인 레인 지속 시간(초)")]
    public float coinRainDuration = 5f;

    [InspectorName("데미지 간격(초)")]
    public float damageInterval = 0.5f;

    [InspectorName("데미지 양")]
    public int damageAmount = 20;

    private Player playerInstance;
    private GameObject activeCoinRain;
    private Camera mainCamera;
    private ParticleSystem particleSystem;

    [Header("Sound Settings")]
    [Tooltip("코인 레인 시작 시 재생할 Wwise 사운드 이벤트")]
    public AK.Wwise.Event coinRainStartSound; // 코인 레인 시작 사운드 이벤트

    [Tooltip("코인 레인 종료 시 재생할 Wwise 사운드 이벤트 (선택사항)")]
    public AK.Wwise.Event coinRainEndSound; // 코인 레인 종료 사운드 이벤트 (선택사항)

    private uint coinRainSoundPlayingID = 0; // 사운드 재생 ID

    // 기존 필드들...
    // (이하 생략)

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

            // 코인 레인 시작 시 사운드 재생
            if (coinRainStartSound != null)
            {
                coinRainSoundPlayingID = coinRainStartSound.Post(activeCoinRain);
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

        // 코인 레인 종료 시 사운드 중지
        if (coinRainStartSound != null && coinRainSoundPlayingID != 0)
        {
            AkSoundEngine.StopPlayingID(coinRainSoundPlayingID);
            coinRainSoundPlayingID = 0;
        }

        // (선택사항) 종료 사운드 재생
        if (coinRainEndSound != null)
        {
            coinRainEndSound.Post(playerInstance.gameObject);
        }
    }

    public override void Upgrade() { }

    public override void ResetLevel()
    {
        lastUsedTime = 0;
    }
}

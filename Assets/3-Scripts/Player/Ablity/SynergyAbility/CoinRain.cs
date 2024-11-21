using System.Collections;
using UnityEngine;
using AK.Wwise; // Wwise ���ӽ����̽� �߰�

[CreateAssetMenu(menuName = "ActiveAbilities/CoinRain")]
public class CoinRain : SynergyAbility
{
    [Header("���κ� �Ķ����")]
    [InspectorName("���� ���� ������")]
    public GameObject coinRainPrefab;

    [InspectorName("���� ���� ���� �ð�(��)")]
    public float coinRainDuration = 5f;

    [InspectorName("������ ����(��)")]
    public float damageInterval = 0.5f;

    [InspectorName("������ ��")]
    public int damageAmount = 20;

    private Player playerInstance;
    private GameObject activeCoinRain;
    private Camera mainCamera;
    private ParticleSystem particleSystem;

    [Header("Sound Settings")]
    [Tooltip("���� ���� ���� �� ����� Wwise ���� �̺�Ʈ")]
    public AK.Wwise.Event coinRainStartSound; // ���� ���� ���� ���� �̺�Ʈ

    [Tooltip("���� ���� ���� �� ����� Wwise ���� �̺�Ʈ (���û���)")]
    public AK.Wwise.Event coinRainEndSound; // ���� ���� ���� ���� �̺�Ʈ (���û���)

    private uint coinRainSoundPlayingID = 0; // ���� ��� ID

    // ���� �ʵ��...
    // (���� ����)

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

            // ���� ���� ���� �� ���� ���
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

        // ���� ���� ���� �� ���� ����
        if (coinRainStartSound != null && coinRainSoundPlayingID != 0)
        {
            AkSoundEngine.StopPlayingID(coinRainSoundPlayingID);
            coinRainSoundPlayingID = 0;
        }

        // (���û���) ���� ���� ���
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

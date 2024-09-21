using UnityEngine;
using System.Collections;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField]
    private ObjectData objectData;

    private int currentHealth;

    private SpriteRenderer spriteRenderer;

    private bool isInvincible = false;

    [SerializeField]
    private GameObject[] spawnPrefabs;

    [SerializeField]
    private float invincibilityDuration = 0.5f;

    [SerializeField]
    private float blinkInterval = 0.1f;

    private void Start()
    {
        if (objectData != null)
        {
            currentHealth = objectData.health;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer가 없습니다. 이 스크립트는 SpriteRenderer가 필요합니다.");
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isInvincible)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                DestroyObject();
            }
            else
            {
                StartCoroutine(InvincibilityCoroutine());
            }
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        for (float i = 0; i < invincibilityDuration; i += blinkInterval)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private void DestroyObject()
    {
        Debug.Log("오브젝트 파괴됨!");

        if (spawnPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, spawnPrefabs.Length);
            Instantiate(spawnPrefabs[randomIndex], transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                TakeDamage(player.stat.currentPlayerDamage);
            }
        }
    }
}

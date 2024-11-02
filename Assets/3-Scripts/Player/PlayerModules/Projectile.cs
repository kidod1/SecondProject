using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Player playerInstance;
    private Vector2 direction;
    private float damageMultiplier = 1.0f;

    [SerializeField]
    protected PlayerData stat;

    private bool isCloneProjectile = false;
    protected Rigidbody2D rb;

    private float lifetime;

    private static Transform projectileParent;

    [SerializeField]
    private int projectileRealDamage;
    public int projectileCurrentDamage = 0; // ���� ������

    [SerializeField]
    private float customSpeed = 10f; // Ŀ���� �ӵ�
    [SerializeField]
    private float customLifetime = 2f; // Ŀ���� ���� �ð�

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D�� �����ϴ�.");
        }

        if (projectileParent == null)
        {
            GameObject parentObj = new GameObject("ProjectileParent");
            projectileParent = parentObj.transform;
        }

        transform.SetParent(projectileParent);
    }

    private void OnEnable()
    {
        if (stat != null)
        {
            // ���� �ð��� Ŀ���� ������ ����
            lifetime = customLifetime;
            Invoke(nameof(Deactivate), lifetime);

            // �ӵ��� Ŀ���� ������ ����
            rb.velocity = direction * customSpeed;
        }
    }

    private void OnDisable()
    {
        CancelInvoke();
        rb.velocity = Vector2.zero;
        isCloneProjectile = false; // ��Ȱ��ȭ �� �ʱ�ȭ
    }

    /// <summary>
    /// ������Ʈ���� ����� �ӵ��� �����մϴ�.
    /// </summary>
    /// <param name="newDirection">������ ����</param>
    /// <param name="speedMultiplier">�ӵ� ���� ����</param>
    public void SetDirection(Vector2 newDirection, float speedMultiplier = 1.0f)
    {
        direction = newDirection.normalized;
        if (rb != null)
        {
            rb.velocity = direction * customSpeed * speedMultiplier;
        }
    }

    /// <summary>
    /// ������Ʈ���� �ʱ�ȭ�մϴ�.
    /// </summary>
    /// <param name="playerStat">�÷��̾��� ������</param>
    /// <param name="playerInstance">�÷��̾� �ν��Ͻ�</param>
    /// <param name="isClone">Ŭ�� ������Ʈ�� ����</param>
    /// <param name="multiplier">������ ����</param>
    /// <param name="damage">������Ʈ���� ������</param>
    /// <param name="speed">������Ʈ���� �ӵ�</param>
    /// <param name="lifetime">������Ʈ���� ���� �ð�</param>
    public void Initialize(PlayerData playerStat, Player playerInstance, bool isClone = false, float multiplier = 1.0f, int damage = 10, float speed = 10f, float lifetime = 2f)
    {
        stat = playerStat;
        this.playerInstance = playerInstance;
        isCloneProjectile = isClone;
        damageMultiplier = multiplier;
        projectileCurrentDamage = damage; // ������ ����

        // Ŀ���� �ӵ��� ���� �ð� ����
        customSpeed = speed;
        customLifetime = lifetime;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (gameObject.activeSelf)
        {
            OnEnable();
        }
    }

    private void Deactivate()
    {
        if (isCloneProjectile)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            Deactivate();
        }
        else
        {
            // Monster�� MidBoss�� �Բ� ó��
            bool isDamageApplied = false;

            // Monster �Ǵ� MidBoss ������Ʈ ��������
            MonoBehaviour damageable = collision.GetComponent<Monster>() as MonoBehaviour;
            if (damageable == null)
            {
                damageable = collision.GetComponent<MidBoss>() as MonoBehaviour;
            }
            if (damageable == null)
            {
                damageable = collision.GetComponentInParent<Monster>() as MonoBehaviour;
            }
            if (damageable == null)
            {
                damageable = collision.GetComponentInParent<MidBoss>() as MonoBehaviour;
            }

            if (damageable != null)
            {
                int damage = Mathf.RoundToInt(projectileCurrentDamage);
                if (isCloneProjectile)
                {
                    damage = Mathf.RoundToInt(damage * damageMultiplier);
                }

                // ������ ���� �� �߰� ���� ó��
                if (damageable is Monster monster)
                {
                    monster.TakeDamage(damage, transform.position);

                    if (playerInstance != null && !isCloneProjectile)
                    {
                        playerInstance.abilityManager.ActivateAbilitiesOnHit(collision);
                    }

                    if (playerInstance != null && playerInstance.CanStun())
                    {
                        float stunChance = 0.25f;
                        if (UnityEngine.Random.value < stunChance)
                        {
                            monster.Stun(2f);
                            Debug.Log($"{monster.name}��(��) �����߽��ϴ�.");
                        }
                    }
                    isDamageApplied = true;
                }
                else if (damageable is MidBoss midBoss)
                {
                    midBoss.TakeDamage(damage, transform.position);

                    if (playerInstance != null && !isCloneProjectile)
                    {
                        playerInstance.abilityManager.ActivateAbilitiesOnHit(collision);
                    }

                    // MidBoss�� ���ϵ��� �ʵ��� ó���ϰų� �ʿ� �� ���� ���� �߰�
                    isDamageApplied = true;
                }
            }

            if (isDamageApplied)
            {
                Deactivate();
            }
            else if (collision.GetComponent<DestructibleObject>() != null)
            {
                // �ı� ������ ������Ʈ ó��
                DestructibleObject destructible = collision.GetComponent<DestructibleObject>();
                if (destructible != null)
                {
                    int damage = Mathf.RoundToInt(projectileCurrentDamage);
                    if (isCloneProjectile)
                    {
                        damage = Mathf.RoundToInt(damage * damageMultiplier);
                    }
                    destructible.TakeDamage(damage);
                }
                Deactivate();
            }
        }
    }
}

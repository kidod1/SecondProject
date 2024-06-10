using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public ObjectPool objectPool;
    [SerializeField]
    private PlayerData stat;
    private PlayerInput playerInput;
    private float shootCooldown = 0.5f; // 발사 쿨타임 (0.5초)
    private float lastShootTime; // 마지막 발사 시간 기록
    private bool isShooting = false;
    private Vector2 shootDirection;
    private Vector2 nextShootDirection; // 다음 공격 방향
    private bool hasNextShootDirection = false; // 다음 공격 방향이 있는지 여부

    public UnityEvent<Vector2, int> OnShoot; // 공격 이벤트

    private void Awake()
    {
        playerInput = new PlayerInput();
        if (OnShoot == null)
        {
            OnShoot = new UnityEvent<Vector2, int>();
        }
    }

    private void OnEnable()
    {
        playerInput.Player.Enable();
        playerInput.Player.Shoot.started += OnShootStarted;
        playerInput.Player.Shoot.performed += OnShootPerformed;
        playerInput.Player.Shoot.canceled += OnShootCanceled;
    }

    private void OnDisable()
    {
        playerInput.Player.Disable();
        playerInput.Player.Shoot.started -= OnShootStarted;
        playerInput.Player.Shoot.performed -= OnShootPerformed;
        playerInput.Player.Shoot.canceled -= OnShootCanceled;
    }

    private void OnShootStarted(InputAction.CallbackContext context)
    {
        Vector2 newDirection = context.ReadValue<Vector2>();
        if (newDirection != Vector2.zero)
        {
            shootDirection = newDirection;
            isShooting = true;

            if (Time.time >= lastShootTime + shootCooldown)
            {
                Shoot(shootDirection, stat.projectileType);
                lastShootTime = Time.time;
            }
        }
    }

    private void OnShootPerformed(InputAction.CallbackContext context)
    {
        Vector2 newDirection = context.ReadValue<Vector2>();
        if (newDirection != Vector2.zero)
        {
            nextShootDirection = newDirection;
            hasNextShootDirection = true;
        }
    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        isShooting = false;
    }

    private void Update()
    {
        if (isShooting && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot(shootDirection, stat.projectileType);
            lastShootTime = Time.time;

            if (hasNextShootDirection)
            {
                shootDirection = nextShootDirection;
                hasNextShootDirection = false;
            }
        }
    }

    private void Shoot(Vector2 direction, int prefabIndex)
    {
        GameObject projectile = objectPool.GetObject(prefabIndex);
        projectile.transform.position = transform.position;
        projectile.GetComponent<Projectile>().SetDirection(direction);

        // 공격 이벤트 발생
        OnShoot.Invoke(direction, prefabIndex);
    }
}
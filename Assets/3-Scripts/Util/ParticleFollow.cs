using UnityEngine;

public class ParticleFollow : MonoBehaviour
{
    [SerializeField]
    private bool usePlayer = true;

    [SerializeField]
    private Transform target;

    public Vector3 offset = Vector3.zero;
    public float followSpeed = 200f; // �ӵ� (���� ���̸� �� ������)

    private Transform player;
    private ParticleSystem particleSystem;
    private ParticleSystem.Particle[] particles;

    void Start()
    {
        if (usePlayer)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("Player �±׸� ���� ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }

        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    void Update()
    {
        Transform followTarget = null;

        // usePlayer�� true�̸� �÷��̾�, false�̸� ������ Ÿ���� ����
        if (usePlayer && player != null)
        {
            followTarget = player;
        }
        else if (!usePlayer && target != null)
        {
            followTarget = target;
        }

        if (followTarget == null) return;  // followTarget�� ������ Update ����

        int numParticlesAlive = particleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 targetPosition = followTarget.position + offset;
            particles[i].position = Vector3.Lerp(particles[i].position, targetPosition, followSpeed * Time.deltaTime);
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}

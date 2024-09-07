using UnityEngine;

public class ParticleFollow : MonoBehaviour
{
    [SerializeField]
    private bool usePlayer = true;

    [SerializeField]
    private Transform target;

    public Vector3 offset = Vector3.zero;
    public float followSpeed = 200f; // 속도 (값을 높이면 더 빨라짐)

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
                Debug.LogError("Player 태그를 가진 오브젝트를 찾을 수 없습니다.");
            }
        }

        particleSystem = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
    }

    void Update()
    {
        Transform followTarget = null;

        // usePlayer가 true이면 플레이어, false이면 지정된 타겟을 따름
        if (usePlayer && player != null)
        {
            followTarget = player;
        }
        else if (!usePlayer && target != null)
        {
            followTarget = target;
        }

        if (followTarget == null) return;  // followTarget이 없으면 Update 중지

        int numParticlesAlive = particleSystem.GetParticles(particles);

        for (int i = 0; i < numParticlesAlive; i++)
        {
            Vector3 targetPosition = followTarget.position + offset;
            particles[i].position = Vector3.Lerp(particles[i].position, targetPosition, followSpeed * Time.deltaTime);
        }

        particleSystem.SetParticles(particles, numParticlesAlive);
    }
}

using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    private void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 파티클 시스템의 재생 시간 후에 오브젝트를 파괴
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                // 애니메이터의 현재 상태 길이 후에 오브젝트를 파괴
                Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
            }
            else
            {
                // 파티클 시스템이나 애니메이터가 없으면 기본 시간 후에 파괴
                Destroy(gameObject, 3f);
            }
        }
    }
}

using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    private void Start()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // ��ƼŬ �ý����� ��� �ð� �Ŀ� ������Ʈ�� �ı�
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                // �ִϸ������� ���� ���� ���� �Ŀ� ������Ʈ�� �ı�
                Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
            }
            else
            {
                // ��ƼŬ �ý����̳� �ִϸ����Ͱ� ������ �⺻ �ð� �Ŀ� �ı�
                Destroy(gameObject, 3f);
            }
        }
    }
}

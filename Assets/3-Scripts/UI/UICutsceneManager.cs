using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UICutsceneManager : MonoBehaviour
{
    [SerializeField]
    private Image cutsceneImage;
    [SerializeField]
    private Sprite[] cutsceneSprites;
    [SerializeField]
    private float fadeDuration = 1f;
    [SerializeField]
    private float displayDuration = 2f;
    [SerializeField]
    private int loadSceneNum = 0;

    private int currentCutsceneIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        if (cutsceneSprites.Length > 0)
        {
            cutsceneImage.sprite = cutsceneSprites[currentCutsceneIndex];
            StartCoroutine(FadeIn());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTransitioning)
        {
            StartCoroutine(NextCutscene());
        }
    }

    private IEnumerator NextCutscene()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeOut());

        currentCutsceneIndex++;

        if (currentCutsceneIndex < cutsceneSprites.Length)
        {
            cutsceneImage.sprite = cutsceneSprites[currentCutsceneIndex];
            yield return StartCoroutine(FadeIn());
        }
        else
        {
            SceneManager.LoadScene(loadSceneNum);
        }

        isTransitioning = false;
    }

    private IEnumerator FadeIn()
    {
        Color color = cutsceneImage.color;
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            float normalizedTime = t / fadeDuration;
            color.a = Mathf.Lerp(0, 1, normalizedTime);
            cutsceneImage.color = color;
            yield return null;
        }
        color.a = 1;
        cutsceneImage.color = color;
    }

    private IEnumerator FadeOut()
    {
        Color color = cutsceneImage.color;
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            float normalizedTime = t / fadeDuration;
            color.a = Mathf.Lerp(1, 0, normalizedTime);
            cutsceneImage.color = color;
            yield return null;
        }
        color.a = 0;
        cutsceneImage.color = color;
    }
}
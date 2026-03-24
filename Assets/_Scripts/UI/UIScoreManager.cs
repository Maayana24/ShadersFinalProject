using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIScoreManager : MonoBehaviour
{
    [SerializeField] private Image filling;
    [SerializeField] private GameObject scorePanel;

    private Coroutine fillCoroutine;

    public static event Action OnRestart;

    private void OnEnable()
    {
        PhotoCompare.OnScore += ShowScore;
        filling.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        PhotoCompare.OnScore -= ShowScore;
    }

    private void ShowScore(float score)
    {
        filling.gameObject.SetActive(true);
        filling.fillAmount = 0;
        scorePanel.SetActive(true);

        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(AnimateFill(score));
    }

    private IEnumerator AnimateFill(float targetScore)
    {
        float currentFill = filling.fillAmount;
        float duration = 1;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            filling.fillAmount = Mathf.Lerp(currentFill, targetScore, elapsed / duration);
            yield return null;
        }

        filling.fillAmount = targetScore;
    }

    public void OnRestartButton()
    {
        DeactivateScorePanel();
        OnRestart?.Invoke();
    }

    public void DeactivateScorePanel()
    {
        filling.gameObject.SetActive(false);
        scorePanel.SetActive(false);
    }

}

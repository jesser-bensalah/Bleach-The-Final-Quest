using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverUI;

    public static GameOverManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de GameOverManager dans la scène");
            return;
        }

        instance = this;
    }

    public void OnPlayerDeath()
    {
        if (CurrentSceneManager.instance.isPlayerPresentByDefault)
        {
            DontDestroyOnLoadScene.instance.RemoveFromDontDestroyOnLoad();
        }
        gameOverUI.SetActive(true);
    }

    public void RetryButton()
    {
        // Désactiver d'abord l'UI
        gameOverUI.SetActive(false);

        // Retirer les pièces si nécessaire
        if (Inventory.instance != null && CurrentSceneManager.instance != null)
        {
            Inventory.instance.RemoveCoins(CurrentSceneManager.instance.coinsPickedUpInThisSceneCount);
        }

        // Recharger la scène
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        // Attendre un peu puis appeler Respawn
        StartCoroutine(RespawnAfterSceneLoad());
    }

    private IEnumerator RespawnAfterSceneLoad()
    {
        // Attendre la fin du chargement de la scène
        yield return new WaitForSeconds(0.1f);

        // Vérifier si PlayerHealth existe
        if (PlayerHealth.instance != null)
        {
            PlayerHealth.instance.Respawn();
        }
    }

    public void MainMenuButton()
    {
        DontDestroyOnLoadScene.instance.RemoveFromDontDestroyOnLoad();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
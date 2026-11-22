using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public static GlobalVariables Instance;
    public static bool _gameIsPaused;

    [SerializeField] private GameObject _pauseScreen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (FindObjectOfType<GameManager>() == null)
        {
            gameObject.AddComponent<GameManager>();
        }
    }

    void Update()
    {
        // Se estiver em Game Over ou no menu inicial de seleção de herói, não abre o menu de pausa por cima
        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || !GameManager.Instance.isCharacterSelected)) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        _gameIsPaused = !_gameIsPaused;

        if (_pauseScreen != null)
        {
            _pauseScreen.SetActive(_gameIsPaused);
        }

        Time.timeScale = _gameIsPaused ? 0f : 1f;
    }
}

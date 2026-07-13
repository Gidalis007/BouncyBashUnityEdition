using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    [SerializeField] private bool _shouldLoadScene = true;
    [SerializeField] private string _sceneName = "GameScene";

    void Start()
    {
        if (_shouldLoadScene)
            SceneManager.LoadScene(_sceneName);
    }
}

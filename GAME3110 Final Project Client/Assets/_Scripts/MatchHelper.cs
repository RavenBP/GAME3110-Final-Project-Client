using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchHelper : MonoBehaviour
{
    private static MatchHelper _instance;
    public static MatchHelper Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public int port = 12345;
    public string ipAddress = "3.130.200.122";

    private void Start()
    {
        SceneManager.sceneLoaded += RunMatch;
    }

    private void RunMatch(Scene scene, LoadSceneMode mode)
    {
        if (GameObject.FindObjectOfType<NetworkMatchLoop>())
        {
            StartCoroutine(NetworkMatchLoop.Instance.StartMatchConnection(ipAddress, port));
        }
    }
}

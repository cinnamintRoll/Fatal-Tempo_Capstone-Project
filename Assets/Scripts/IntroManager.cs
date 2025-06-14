using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private GameObject IntroMenu;

    [SerializeField] private CharacterController controller;

    private string SkipKey = "GeneratorIsOn";

    // Start is called before the first frame update
    void Start()
    {
        bool GameStart = PlayerPrefs.GetInt(SkipKey) != 0;

        if (GameStart) {
            IntroMenu.SetActive(false);
        }
        else
        {
            Debug.Log("Movement Disabled");
            controller.enabled = false;
        }
    }

    public void closeMenu()
    {
        controller.enabled = true;
        IntroMenu?.SetActive(false);    
    }


    public void SkipIntro()
    {
        PlayerPrefs.SetInt(SkipKey, 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public void PlayLevel(int levelNumber)
    {
        SceneManager.LoadScene(1 + levelNumber);
    }

    public void MenuButton()
    {
        SceneManager.LoadScene(0);
    }
}

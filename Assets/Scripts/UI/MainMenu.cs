using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    Button newGamge;
    Button continueGame;
    Button quit;
    PlayableDirector director;
    private void Awake()
    {
        newGamge = transform.GetChild(1).GetComponent<Button>();
        continueGame = transform.GetChild(2).GetComponent<Button>();
        quit = transform.GetChild(3).GetComponent<Button>();
        newGamge.onClick.AddListener(PlayTimeline);
        continueGame.onClick.AddListener(ContinueGame);
        quit.onClick.AddListener(QuitGame);

        director = FindObjectOfType<PlayableDirector>();
        director.stopped += NewGame;
    }
    void PlayTimeline()
    {
        director.Play();
    }
    void NewGame(PlayableDirector obj)
    {
        PlayerPrefs.DeleteAll();
        SceneController.Instance.TransitionToFirstLevel();
    }
    void ContinueGame()
    {
        SceneController.Instance.TransitionToLoadGame();
    }
    void QuitGame()
    {
        Application.Quit();
        Debug.Log("ÍË³öÓÎÏ·");
    }


}

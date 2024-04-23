using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>,IEndGameObserver
{
    public GameObject playerPrefab;
    public SceneFader SceneFaderPrefab;
    private GameObject player;
    NavMeshAgent playerAgent;
    bool fadeFinshed;
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    private void Start()
    {
        GameManager.Instance.AddObserver(this);
        fadeFinshed = true;
    }
    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
                break;
        }           
    }
    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        SaveManager.Instance.SavePlayerData();
        if (SceneManager.GetActiveScene().name == sceneName)
        {        
            player = GameManager.Instance.playerStates.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            playerAgent.enabled = true; ;
            yield return null;
        }
        else
        {
            yield return SceneManager.LoadSceneAsync(sceneName);
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            SaveManager.Instance.LoadPlayerData();
            yield break;
        }
        
    }
    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        var entrances = FindObjectsOfType<TransitionDestination>();
        for(int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].destinationTag == destinationTag)
            {
                return entrances[i];
            }
        }

        return null;
    }

    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Game"));
    }
    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }
    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }
    IEnumerator LoadLevel(string scene)
    {
        fadeFinshed = true;
        SceneFader fade = Instantiate(SceneFaderPrefab);
        if (scene != "")
        {
            yield return StartCoroutine(fade.FadeOut(2.5f));
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab,GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);
            SaveManager.Instance.SavePlayerData();
            yield return StartCoroutine(fade.FadeIn(2.5f));
            yield break;
        }
    }
    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(SceneFaderPrefab);
        yield return StartCoroutine(fade.FadeOut(2.5f));
        yield return SceneManager.LoadSceneAsync("Main");
        yield return StartCoroutine(fade.FadeIn(2.5f));
        yield break;
    }

    public void EndVotify()
    {
        if (fadeFinshed)
        {
            fadeFinshed = false;
            StartCoroutine(LoadMain());
        } 
    }
}

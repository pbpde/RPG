using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameManager : Singleton<GameManager>
{

    public CharacterStats playerStates;

    public CinemachineFreeLook followCamera;

    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }
    public void RigsterPlayer(CharacterStats player)
    {
        playerStates = player;
        followCamera = FindAnyObjectByType<CinemachineFreeLook>();
        if (followCamera != null)
        {
            followCamera.Follow = playerStates.transform;
            followCamera.LookAt = playerStates.transform;
        }

    }

    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer);
    }

    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }
    public void NotifyObservers()
    {
        foreach(var observer in endGameObservers)
        {
            observer.EndVotify();
        }
    }

    public Transform GetEntrance()
    {
        foreach(var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag == TransitionDestination.DestinationTag.ENTER)
            {
                return item.transform;
            }
        }
        return null;
    }
}

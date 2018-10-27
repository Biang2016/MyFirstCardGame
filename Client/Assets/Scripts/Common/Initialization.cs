﻿using UnityEngine;

public class Initialization : MonoSingleton<Initialization>
{
    private Initialization()
    {
    }

    [SerializeField] private GameObject Manager;

    void Awake()
    {
        Instantiate(Manager);
        ClientLog.Instance.PrintClientStates("Start the client...");
    }
}
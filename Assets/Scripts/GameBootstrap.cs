using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameBootstrap : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameState gameState;
    private GameController gameController;
    public Tilemap map;
    void Awake()
    {
        gameState = new GameState(map);
        gameController = new GameController(gameState);
    }

    // Update is called once per frame

}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BoardClickDetecter : MonoBehaviour
{
    [SerializeField] private Camera boardCamera;
    [SerializeField] private Tilemap terrainTilemap;
    public event Action<Vector3Int> CellClicked;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame

    void Update()
    {
        Mouse mouse = Mouse.current;

        if(mouse==null||!mouse.leftButton.wasPressedThisFrame) return;
        Vector2 screenPosition = mouse.position.ReadValue();
        Ray ray = boardCamera.ScreenPointToRay(screenPosition);

        Plane clickedPlane = new Plane(
            terrainTilemap.transform.forward,
            terrainTilemap.transform.position
        );
        if(!clickedPlane.Raycast(ray,out float distance)) return;

        Vector3 worldPosition = ray.GetPoint(distance);
        Vector3Int cell = terrainTilemap.WorldToCell(worldPosition);

        CellClicked?.Invoke(cell);

    }
}

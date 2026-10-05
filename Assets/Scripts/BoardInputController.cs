using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BoardInputController : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame

    [SerializeField]public BoardClickDetecter boardClickDetecter;

    private void OnEnable()
    {
        boardClickDetecter.CellClicked += HandleCellClicked;
    }

    private void OnDisable()
    {
        boardClickDetecter.CellClicked -= HandleCellClicked;
    }

    void Update()
    {


    }

    private void HandleCellClicked(Vector3Int cellPos)
    {
        Debug.Log($"cell was clicked! {cellPos}");
    }
}

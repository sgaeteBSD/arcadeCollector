using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] public LayerMask solidObjectsLayer;
    [SerializeField] public LayerMask interactableLayer;
    [SerializeField] public LayerMask playerLayer;

    public static GameLayers i { get; set; }
    private void Awake()
    {
        i = this;
    }

    public LayerMask PlayerLayer
    {
        get => playerLayer;
    }
    public LayerMask SolidLayer
    {
        get => solidObjectsLayer;
    }
    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }
}

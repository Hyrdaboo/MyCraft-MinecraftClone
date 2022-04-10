using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public GameObject highlighter;
    public Sprite itemSprite;
    public Image itemHolder;
    public BlockType blockOutput;
    public bool active = false;

    private void Start()
    {
        itemHolder.sprite = itemSprite;
    }

    private void Update()
    {
        if (active)
        {
            highlighter.SetActive(true);
        }
        else
        {
            highlighter.SetActive(false);
        }
    }
}

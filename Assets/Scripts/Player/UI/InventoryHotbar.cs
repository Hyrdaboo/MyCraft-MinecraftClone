using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryHotbar : MonoBehaviour
{
    public InventorySlot[] allSlots;
    public modifyTerrain player;

    public int activeSlot = 0;
    private void Start()
    {
        allSlots = gameObject.GetComponentsInChildren<InventorySlot>();
    }

    private void Update()
    {
        if (activeSlot < 0) activeSlot = allSlots.Length-1;
        if (activeSlot >= allSlots.Length) activeSlot = 0;

        foreach (InventorySlot slot in allSlots)
        {
            slot.active = false;
        }
        allSlots[activeSlot].active = true;
        player.blockToPlace = allSlots[activeSlot].blockOutput;
        SetInventoryIndex();
        
        if (Input.mouseScrollDelta.y > 0) activeSlot--;
        if (Input.mouseScrollDelta.y < 0) activeSlot++;
    }
    
    void SetInventoryIndex()
    {
        if (Input.anyKeyDown)
        {
            int outVal = -1;
            try
            {
                outVal = Convert.ToInt32(Input.inputString);
            }
            catch (Exception e)
            {
                return;
            }
            if (outVal - 1 >= allSlots.Length || outVal - 1 < 0) return;
            activeSlot = outVal-1;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public MapGenerator mapGen;
    public TMP_InputField seedInput;
    public Canvas screenUI;
    public GameObject terrain;
    public FirstPerson player;
    public GameObject main;
    public GameObject msg;
    public Slider slider;

    int seed;
    float viewDist = 45;
    float maxViewDist = 100;
    private void Start()
    {
        seedInput.text = mapGen.seed.ToString();
    }

    public void SetRandomSeed()
    {
        seed = Random.Range(-100000, 100000);
        mapGen.seed = seed;
        seedInput.text = seed.ToString();
    }
    public void SetSeed()
    {
        try
        {
            seed = System.Convert.ToInt32(seedInput.text);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.ToString());
            seed = seedInput.text.GetHashCode();
        }
        mapGen.seed = seed;
    }
    public void Generate()
    {
        terrain.SetActive(true);
        main.SetActive(false);
        msg.SetActive(true);
    }
    public void SetViewDist()
    {
        viewDist = slider.value * maxViewDist;
        EndlessTerrain.maxViewDst = viewDist;
    }
    private void Update()
    {
        if (EndlessTerrain.finished && player.enabled == false)
        {
            screenUI.gameObject.SetActive(true);
            player.enabled = true;
            gameObject.SetActive(false);
        }
    }
}

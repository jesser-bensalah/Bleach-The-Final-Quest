using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public int coinsCount;
    public Text coinsCountText;

    public static Inventory instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Il y a plus d'une instance de Inventory dans la scène");
            return;
        }

        instance = this;
    }

   


    public void AddCoins(int count)
    {
        coinsCount += count;
        coinsCountText.
            text = coinsCount.ToString();
    }
    public void RemoveCoins(int count)
    {
        coinsCount -= count;
        coinsCountText.
             text = coinsCount.ToString();
    }



}

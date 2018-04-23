using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCodeGenerator : MonoBehaviour {

    public string[] codes = new string[] { "giraffe", "camel", "tiger", "lion", "zebra", "leopard" };
    public int maxIntegerValue = 1000;

    public string Generate()
    {
        return Random.Range(0, maxIntegerValue) + "_" + codes[Random.Range(0, codes.Length)];
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class menuplay : MonoBehaviour
{
    public void OnMouseDown()
    {
        SceneManager.LoadScene("SampleScene");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void GetJson()
    {
        WorldGenerator worldGenerator = FindObjectOfType<WorldGenerator>();

        string result = worldGenerator.GetMapToJson();

        inputField.text = result;

        print(result);
    }


    public void LoadNewMap()
    {
        PlayerPrefs.SetString("mode", "new");
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadMapByJson()
    {
        PlayerPrefs.SetString("mode", inputField.text);
        SceneManager.LoadScene("SampleScene");
    }

}

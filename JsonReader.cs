using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class JsonReader : MonoBehaviour
{
    #region Public Data Members

    public TextAsset jsonFile;
    public GameObject PanelAd;
    public GameObject messagePanel;
    public Image imageAd;
    public Text textAd;
    public Text textError;
    public TMP_InputField inputMessage;
    public string jsonPath;

    #endregion

    #region Private Data Members

    private TextAsset json;
    private string texturePath;
    private Color color = Color.black;

    #endregion

    #region Greedy Ad Classes

    [System.Serializable]
    class Position
    {
        public int x;
        public int y;
        public int width;
        public int height;
    }

    [System.Serializable]
    class Operation
    {
        public string name;
        public string argument;
    }

    [System.Serializable]
    class Placements
    {
        public Position position;
    }

    [System.Serializable]
    class Layers
    {
        public string type;
        public string path;
        public List<Placements> placement;
        public List<Operation> operations;
    }

    [System.Serializable]
    class Layer
    {
        public List<Layers> layers;
    }

    #endregion

    #region Unity Callbacks

    void Start()
    {
        Layer AdLayersInJson;

        // saving text from given url in a test.txt text file
        if (string.IsNullOrEmpty(jsonPath) == false)
        {
            StartCoroutine(GetJsonData(jsonPath));
        }

        AdLayersInJson = JsonUtility.FromJson<Layer>(jsonFile.text);

        foreach (Layers layer in AdLayersInJson.layers)
        {
            if(layer == null)
            {
                return;
            }

            if(layer.type != null)
            {
                if (layer.path != null)
                {
                    texturePath = layer.path;
                }

                // code for placements and positions
                if (layer.placement != null)
                {
                    foreach (Placements placement in layer.placement)
                    {
                        bool passed = Uri.TryCreate(texturePath, UriKind.Absolute, out Uri uriResult) &&
                            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                        if (texturePath != null && passed)
                        {
                            StartCoroutine(GetTexture(texturePath));
                            textError.text = string.Empty;
                        }
                        else
                        {
                            textError.text = "Template Is Unavailable";
                        }

                        imageAd.transform.localPosition = new Vector2(placement.position.x, placement.position.y);
                        imageAd.rectTransform.sizeDelta = new Vector2(placement.position.width, placement.position.height);

                    }
                }

                // code for opertaions
                if (layer.operations != null)
                {
                    foreach (Operation operation in layer.operations)
                    {
                        if (operation.name == "color")
                        {
                            if (ColorUtility.TryParseHtmlString(operation.argument, out color))
                            {
                                if(layer.type == "text")
                                {
                                    textAd.color = new Color(color.r, color.b, color.b, 1f);
                                }
                                else
                                {
                                    PanelAd.GetComponent<Image>().color = new Color(color.r, color.b, color.b, 0.6f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    #endregion

    #region Utility Member Methods

    IEnumerator GetJsonData(string path)
    {
        UnityWebRequest jsonRequest = UnityWebRequest.Get(path);
        yield return jsonRequest.SendWebRequest();

        File.WriteAllText("Assets/Resources/test.txt", jsonRequest.downloadHandler.text);
    }

    IEnumerator GetTexture(string path)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);

        yield return www.SendWebRequest();

        Texture2D texture = DownloadHandlerTexture.GetContent(www);

        if(texture != null && CheckIfImageExists(texture))
        {
            textError.text = string.Empty;
            imageAd.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        }
        else
        {
            textError.text = "Template Is Unavailable";
        }
    }

    bool CheckIfImageExists(Texture imageToCheck)
    {
        return imageToCheck.width > 10 && imageToCheck.height > 10;
    }

    public void ShowAd()
    {
        if(string.IsNullOrEmpty(inputMessage.text))
        {
            textAd.text = "No Ad Text Available";
        }
        else
        {
            textAd.text = inputMessage.text;
        }

        inputMessage.text = string.Empty;

        messagePanel.SetActive(false);
        imageAd.gameObject.SetActive(true);
    }
    #endregion
}
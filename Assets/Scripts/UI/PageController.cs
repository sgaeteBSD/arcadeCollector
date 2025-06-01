using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PageController : MonoBehaviour
{
    [SerializeField] private Transform modelParent;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text genText;
    [SerializeField] private TMP_Text foundText;
    [SerializeField] private Material silhouetteMaterial;

    private GameObject currentModel;
    public void SetPage(CarouselItemData data)
    {
        if (currentModel != null) { 
            Destroy(currentModel);
        }

        if (data.largeModel)
        {
            currentModel = Instantiate(data.largeModel, modelParent);
            currentModel.transform.localScale = Vector3.one * 1f; //adjust scale here
            if (data.ud == true) 
            {
                currentModel.transform.localPosition = new Vector3(0.25f,3.6f,0f); //adjust positioning
            }
            else {
                if (data.gen == "GAMERS")
                {
                    currentModel.transform.localPosition = new Vector3(0.25f, 0f, 0f); //adjust positioning
                }
                else
                {
                    currentModel.transform.localPosition = Vector3.zero; //adjust positioning
                }
            }

            var swapper = currentModel.GetComponent<MaterialSwapper>();
            if (swapper != null)
            {
                if (data.isObtained)
                    swapper.RestoreMaterials();
                else
                    swapper.SetSilhouette();
            }
        }
        if (data.isObtained)
        {
            //mainImage.texture = data.largeModelTex;
            //mainImage.color = Color.white;
            nameText.text = data.name;
            descText.text = data.desc;
            foundText.text = data.found;
            genText.text = data.gen;
        }
        else
        {
            //mainImage.texture = data.largeModelTex;
            //mainImage.color = Color.black; //image.color = new Color(0, 0, 0, 0.5f)
            nameText.text = "???";
            foundText.text = "???";
            genText.text = data.gen;
            descText.text = data.hint;
        }
    }
}

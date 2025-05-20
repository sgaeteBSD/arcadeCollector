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
            currentModel.transform.localPosition = Vector3.zero; //adjust positioning
        }
        if (data.isObtained)
        {
            //mainImage.texture = data.largeModelTex;
            //mainImage.color = Color.white;
            nameText.text = data.name;
            descText.text = data.desc;
        }
        else
        {
            //mainImage.texture = data.largeModelTex;
            //mainImage.color = Color.black; //image.color = new Color(0, 0, 0, 0.5f)
            nameText.text = "???";
            descText.text = data.hint;
        }
    }
}

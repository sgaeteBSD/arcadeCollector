using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarouselSlot : MonoBehaviour
{
    [SerializeField] private Image image;
    private CarouselItemData data;

    public void Setup(CarouselItemData itemData)
    {
        data = itemData;
        image.sprite = data.smallSprite;
        if (data.isObtained)
        {
            image.color = Color.white;
        }
        else
        {
            image.color = Color.black;
        }
    }

    public CarouselItemData GetData()
    {
        return data;
    }
}

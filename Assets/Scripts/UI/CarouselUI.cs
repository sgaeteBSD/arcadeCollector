using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CarouselUI : MonoBehaviour
{
    [SerializeField] private RectTransform carContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int visibleSlots = 7; //make sure this is odd
    [SerializeField] private List<CarouselItemData> items;
    [SerializeField] private PageController pageController;

    private int selectedIndex = 0;
    private List<GameObject> currentSlots = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        CollectionManager.Instance.LoadCollection(); // ensure fresh load
        Redraw();
        UpdatePage();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.activeInHierarchy) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (selectedIndex < items.Count - 1)
            {
                selectedIndex++;
                Redraw();
                UpdatePage();
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (selectedIndex > 0)
            {
                selectedIndex--;
                Redraw();
                UpdatePage();
            }
        }
    }

    void Redraw()
    {
        foreach (Transform child in carContainer)
        {
            Destroy(child.gameObject); // clear existing
        }
        currentSlots.Clear();

        int half = visibleSlots / 2;
        for (int i = -half; i <= half; i++)
        {
            int index = selectedIndex + i;
            GameObject slotGO = Instantiate(slotPrefab, carContainer);
            Image img = slotGO.GetComponent<Image>();

            if (index >= 0 && index < items.Count)
            {
                var item = items[index];
                bool collected = CollectionManager.Instance.HasPrize(item.itemID); // Ensure CarouselItemData has an itemID field
                if (collected)
                {
                    item.isObtained = true;
                }

                img.sprite = collected ? item.smallSprite : item.silhouetteSprite;
                img.color = collected ? Color.white : new Color(0, 0, 0, 0.5f); // Slight dimming for unobtained
            }
            else
            {
                img.sprite = null;
                img.color = new Color(1, 1, 1, 0); //hide transparent
            }

            //scale selected
            float scale = (i == 0) ? 1.2f : 0.8f;
            slotGO.transform.localScale = Vector3.one * scale;
            currentSlots.Add(slotGO);
        }
    }
    void UpdatePage()
    {
        if (pageController != null && selectedIndex >= 0 && selectedIndex < items.Count)
            pageController.SetPage(items[selectedIndex]);
    }
}

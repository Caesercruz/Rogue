using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject slidesParent;
    [SerializeField] private int sliderIndex = 1;
    [SerializeField] private TextMeshProUGUI sliderNumber;

    private void Update()
    {
        if (transform.parent.GetComponent<GameScript>().GameControls.Actions.Back.triggered) transform.parent.GetComponent<GameScript>().CloseAction();
    }
    public void NextSlide()
    {
        if (sliderIndex < 4)
        {
            slidesParent.transform.Find($"Slide {sliderIndex}").transform.localPosition += new Vector3(0, -18, 0);
            sliderIndex++;
            sliderNumber.text = $"{sliderIndex}/4";
            slidesParent.transform.Find($"Slide {sliderIndex}").transform.localPosition += new Vector3(0, 18, 0);
        }
    }
    public void PreviousSlide()
    {
        if (sliderIndex > 1)
        {
            slidesParent.transform.Find($"Slide {sliderIndex}").transform.localPosition += new Vector3(0, -18, 0);
            sliderIndex--;
            sliderNumber.text = $"{sliderIndex}/4";
            slidesParent.transform.Find($"Slide {sliderIndex}").transform.localPosition += new Vector3(0, 18, 0);
        }
    }
}

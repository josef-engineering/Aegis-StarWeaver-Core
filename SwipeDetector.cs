using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeDetector : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public ModelViewController controller; 
    public float threshold = 50f; 

    public void OnDrag(PointerEventData eventData)
    {
        // Unity needs this empty method to know we are listening for drags
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        float difference = eventData.pressPosition.x - eventData.position.x;
        Debug.Log($"[SwipeDetector] Drag detected. Distance: {difference}");

        if (Mathf.Abs(difference) > threshold)
        {
            if (difference > 0)
            {
                Debug.Log("<color=green>SWIPE LEFT -> Next Slide</color>");
                if(controller != null) controller.NextSlide();
            }
            else
            {
                Debug.Log("<color=green>SWIPE RIGHT -> Previous Slide</color>");
                if(controller != null) controller.PreviousSlide();
            }
        }
    }
}
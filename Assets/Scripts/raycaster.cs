using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RaycastLogger : MonoBehaviour
{
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;

    void Start()
    {
        if (raycaster == null)
            raycaster = GetComponent<GraphicRaycaster>();

        if (eventSystem == null)
            eventSystem = FindObjectOfType<EventSystem>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            Debug.Log("Raycast Hit:");
            foreach (RaycastResult result in results)
            {
                Debug.Log(result.gameObject.name);
            }
        }
    }
}

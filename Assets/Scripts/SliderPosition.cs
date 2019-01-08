using UnityEngine;

public class SliderPosition : MonoBehaviour
{
    public GameObject Player; //Главный персонаж
    public Vector3 Offset; // Смещение слайдера от модели главного персонажа

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(Player.transform.position + Offset);
    }
}
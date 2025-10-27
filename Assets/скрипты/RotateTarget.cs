using UnityEngine;

public class RotateTarget : MonoBehaviour
{
    [Header("Объект для вращения")]
    [Tooltip("Перетащите сюда вашу монету или любой другой объект")]
    public Transform target;

    [Header("Параметры вращения")]
    [Tooltip("Ось вращения (в единицах Vector3). Обычно (0,1,0) — вокруг вертикали")]
    public Vector3 rotationAxis = Vector3.up;

    [Tooltip("Скорость вращения в градусах в секунду")]
    public float rotationSpeed = 180f;

    

    void Update()
    {
        if (target != null)
        {
            // вращаем target вокруг своей оси
            target.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}

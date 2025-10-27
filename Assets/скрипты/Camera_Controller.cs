using UnityEngine;
using System.Collections;

public class Camera_Controller : MonoBehaviour
{
    [Header("Transforms for camera states")]
    [Tooltip("Transform for gameplay view (child of moving parent)")]
    public Transform gameViewTransform;
    [Tooltip("Transform for menu/end-game view (child of moving parent)")]
    public Transform menuViewTransform;

    [Header("Transition Settings")]
    [Tooltip("Duration of camera transition in seconds")]
    public float transitionDuration = 1f;

    private Coroutine transitionCoroutine;

    /// <summary>
    /// Moves camera smoothly to gameplay position using local coordinates.
    /// </summary>
    public void StartGameCamera()
    {
        if (gameViewTransform != null)
            MoveToTransform(gameViewTransform, useLocal: true);
        else
            Debug.LogWarning("Game View Transform is not assigned.");
    }

    /// <summary>
    /// Moves camera smoothly to menu/end-game position using local coordinates.
    /// </summary>
    public void ReturnCamera()
    {
        if (menuViewTransform != null)
            MoveToTransform(menuViewTransform, useLocal: true);
        else
            Debug.LogWarning("Menu View Transform is not assigned.");
    }

    private void MoveToTransform(Transform target, bool useLocal = false)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(TransitionCoroutine(target, useLocal));
    }

    private IEnumerator TransitionCoroutine(Transform target, bool useLocal)
    {
        Vector3 startPos = useLocal ? transform.localPosition : transform.position;
        Quaternion startRot = useLocal ? transform.localRotation : transform.rotation;
        Vector3 endPos = useLocal ? target.localPosition : target.position;
        Quaternion endRot = useLocal ? target.localRotation : target.rotation;

        float elapsed = 0f;
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / transitionDuration);
            if (useLocal)
            {
                transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            }
            else
            {
                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }
            yield return null;
        }
        if (useLocal)
        {
            transform.localPosition = endPos;
            transform.localRotation = endRot;
        }
        else
        {
            transform.position = endPos;
            transform.rotation = endRot;
        }
        transitionCoroutine = null;
    }
}

using UnityEngine;
using System.Collections;

public class DestroyOnAwake : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject);
    }
}

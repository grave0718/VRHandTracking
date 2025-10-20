using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ForceLocalZero : MonoBehaviour
{
    void LateUpdate()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
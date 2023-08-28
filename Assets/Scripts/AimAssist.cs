using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AimAssist
{
    private LayerMask mask;

    void Start()
    {
        mask = LayerMask.GetMask("Ground", "NotPass");
    }

    void Update()
    {
        Ray();

    }

    void Ray()
    {
       // RaycastHit2D hit = Physics2D.Raycast(this.transform.position, aimVec, Mathf.Infinity, mask);
    }
}


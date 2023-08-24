using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace W02
{
    public class PlayerMove : PlayerAbility
    {
        protected override void HandleInput()
        {
            transform.Translate(_horizontalMove * Time.deltaTime, 0, 0);
            Debug.Log(_horizontalAim + ", " +  _verticalAim);
        }
    }
}



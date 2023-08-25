using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRopeSwing : PlayerAbility
{
    protected override void HandleInput()
    {
        if (_hookButtonClicked && _player.playerInfo.ropeState == Player.RopeState.HOLDING)
        {
            
        }
    }
}

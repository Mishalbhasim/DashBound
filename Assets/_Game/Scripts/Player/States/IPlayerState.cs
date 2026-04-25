using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerState
{
    void Enter(PlayerStateMachine sm);
    void Update(PlayerStateMachine sm);
    void FixedUpdate(PlayerStateMachine sm);
    void Exit(PlayerStateMachine sm);
}

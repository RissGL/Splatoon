using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface  IMoveState
{
    void OnEnter(MoveSystem moveSystem);

    void OnUpdate(MoveSystem moveSystem);
    
    void OnExit(MoveSystem moveSystem);
}

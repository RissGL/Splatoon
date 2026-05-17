using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerState 
{
    public void OnEnter(MoveSystem moveSystem);

    public void OnUpdate(MoveSystem moveSystem);

    public void OnExit(MoveSystem moveSystem);
}

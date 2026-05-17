using UnityEngine;

[CreateAssetMenu(menuName = "Data/Movement Params Set")]
public class MovementParamsSet : ScriptableObject
{
    public StateMovementPair[] stateMovementConfigs;

    public MovementParams GetMovementParams(PlayerMovementState state)
    {
        foreach (var config in stateMovementConfigs)
            if (config.state == state) return config.parameters;
        return default;
    }
}

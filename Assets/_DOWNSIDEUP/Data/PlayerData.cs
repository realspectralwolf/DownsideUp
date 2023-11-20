using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    [Header("Movement")]
    public float WalkVelocity;
    public float HoldingWalkVelocity;
    public float JumpForce;
    public float GroundCheckRadius = 0.3f;
    public LayerMask GroundLayer;
    public float RotSmoothness;

    [Header("Grab System")]
    public float TargetForwardBias = 0.5f;
    public Color32 ClosestItemColor;
    public Color32 HeldItemColor;
    public float HeldItemAdditionalGravity = 0.6f;
    public Vector3 SpringStiffness = new Vector3(0.1f, 0.1f, 0.1f);
    public int DraggableLayerID;
}

using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ThirdPersonCharacter))]
[RequireComponent(typeof(PlayerCameraController))]
public class PlayerMovementSpeedChanger : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    [Tooltip("It moves the character with x% of its total movement speed. If this is 0 then the character cannot move.")]
    [SerializeField] float snipingMoveSpeedDiminisher = 0.4f;

    private ThirdPersonCharacter thirdPersonCharacter;
    private PlayerCameraController cameraController;

    // Use this for initialization
    void Start()
    {
        thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
        cameraController = GetComponent<PlayerCameraController>();
    }

    // Update is called once per frame
    void Update() => thirdPersonCharacter.movementSpeedMultiplier = cameraController.isSniping ? snipingMoveSpeedDiminisher : 1f;
}

using UnityEngine;
using UnityEngine.InputSystem;
using Unity.MLAgents.Input;

public class InputAssetProviderBridge : MonoBehaviour, IInputActionAssetProvider
{
    private PlayerInput playerInput;

    public (InputActionAsset, IInputActionCollection2) GetInputActionAsset()
    {
        if (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
        }
        
        return (playerInput.actions, playerInput.actions);
    }
}

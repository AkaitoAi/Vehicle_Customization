using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizer : MonoBehaviour
{
    [SerializeField] private Button enterCustomizationButton, exitCustomizationButton;
    [SerializeField] private Transform standPosition;
    [SerializeField] private string animToEnter = "ClothChangeEnter";
    [SerializeField] private string animToExit = "ClothChangeExit";
    [SerializeField] private float setupDelay = .75f;
    [SerializeField] private GameObject[] toHide, toShow;

    private vThirdPersonController _playerController;
    private vThirdPersonInput _playerInput;
    private WeaponEquipController _playerWeaponController;
    private Transform _playerTransform;
    private Rigidbody _playerRigidbody;
    private Collider _playerCollider;
    private Animator _playerAnimator;
    private int weaponID = -1;

    private const string _customizationMainCameraState = "CustomizationDefault";
    private const string _defaultCameraState = "Default";



    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out _playerController))
            return;

        _playerTransform = _playerController.transform;
        _playerRigidbody = _playerController._rigidbody;
        _playerCollider = _playerController._capsuleCollider;
        _playerAnimator = _playerController.animator;

        if (enterCustomizationButton != null)
        {
            enterCustomizationButton.onClick.RemoveAllListeners();
            enterCustomizationButton.onClick.AddListener(EnterCustomization);
            enterCustomizationButton.gameObject.SetActive(true);
        }
        if (exitCustomizationButton != null)
        {
            exitCustomizationButton.onClick.RemoveAllListeners();
            exitCustomizationButton.onClick.AddListener(ExitCustomization);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out _playerController))
            return;

        if (enterCustomizationButton != null)
        {
            enterCustomizationButton.gameObject.SetActive(false);
            enterCustomizationButton.onClick.RemoveAllListeners();
        }

        if(exitCustomizationButton != null) exitCustomizationButton.onClick.RemoveAllListeners();

        _playerTransform = null;
        _playerRigidbody = null;
        _playerCollider = null;
        _playerAnimator = null;
        _playerController = null;
    }

    [ContextMenu("Enter Customization")]
    private void EnterCustomization()
    {
        //TODO Hide UI
        foreach (GameObject g in toHide) g.SetActive(false);

        //TODO Unequip Weapons
        if (_playerTransform.TryGetComponent(out _playerWeaponController))
        {
            weaponID = _playerWeaponController.CurrentSelectedID;
            _playerWeaponController.UnEquipWeapon();
        }

        //TODO Hide Mobile
        Invoke("Enter", setupDelay);
    }
    private void Enter()
    {
        //TODO Reset current cloths

        //TODO Cloth change UI
        foreach (GameObject g in toShow) g.SetActive(true);

        //TODO Toggle Cloth camera Change Camera State
        if (_playerTransform.TryGetComponent(out _playerInput))
            _playerInput.ChangeCameraState(_customizationMainCameraState, true);

        if (_playerRigidbody != null)
        {
            _playerRigidbody.isKinematic = true;
            _playerRigidbody.useGravity = false;
        }

        if (_playerCollider != null) _playerCollider.enabled = false;
        if (_playerAnimator != null) _playerAnimator.Play(animToEnter);
        if (_playerTransform != null && standPosition != null) _playerTransform.SetPositionAndRotation(new Vector3(standPosition.position.x,
            _playerTransform.position.y, standPosition.position.z), standPosition.rotation);
    }

    [ContextMenu("Exit Customization")]
    private void ExitCustomization()
    {
        //TODO Hide Cloth UI
        foreach (GameObject g in toShow) g.SetActive(false);

        //TODO Save Equipped cloths

        //TODO Show Interstitial

        Invoke("Exit", setupDelay);
    }
    private void Exit()
    {
        //TODO Show UI
        foreach (GameObject g in toHide) g.SetActive(true);

        if (_playerRigidbody != null)
        {
            _playerRigidbody.isKinematic = false;
            _playerRigidbody.useGravity = true;
        }
        if (_playerCollider != null) _playerCollider.enabled = true;
        if (_playerAnimator != null) _playerAnimator.Play(animToExit);

        //TODO Toggle Camera Change Camera State
        if (_playerTransform.TryGetComponent(out _playerInput))
            _playerInput.ChangeCameraState(_defaultCameraState, false);

        //TODO Equip Weapons
        if (weaponID == -1) return;
        if (_playerTransform.TryGetComponent(out _playerWeaponController))
            _playerWeaponController.EquipWeapon(weaponID);

        weaponID = -1;
    }
}

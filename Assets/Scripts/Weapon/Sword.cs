using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapon
{
    [SerializeField] private GameObject slashAnimationPrefab;
    [SerializeField] private Transform slashAnimationSpawnPoint;
    //[SerializeField] private float swordAttackCD = 0.5f;
    [SerializeField] private WeaponInfo weaponInfo;

    private Transform weaponCollider;
    private Animator myAnimator;
    private GameObject slashAnimation;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        weaponCollider = PlayerController.Instance.GetWeaponCollider();
        slashAnimationSpawnPoint = GameObject.Find("SlashSpawnPoint").transform;

        this.transform.parent = PlayerController.Instance.transform;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;

        ActiveWeapon.Instance.NewWeapon(this);
    }



    private void Update()
    {
        MouseFollowWithOffset();
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    public void Attack()
    {
        myAnimator.SetTrigger("Attack");
        weaponCollider.gameObject.SetActive(true);
        slashAnimation = Instantiate(slashAnimationPrefab, slashAnimationSpawnPoint.position, Quaternion.identity);
        slashAnimation.transform.parent = this.transform.parent;

    }


    public void DoneAttackingAnimEvent()
    {
        weaponCollider.gameObject.SetActive(false);
    }


    public void SwingUpFlipAnim()
    {
        slashAnimation.gameObject.transform.rotation = Quaternion.Euler(-180, 0, 0);
        
        if (PlayerController.Instance.FacingLeft)
        {
            slashAnimation.GetComponent<SpriteRenderer>().flipX = true;
        }

    }

    public void SwingDownFlipAnim()
    {
        slashAnimation.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        if (PlayerController.Instance.FacingLeft)
        {
            slashAnimation.GetComponent<SpriteRenderer>().flipX = true;
        }
    }

    private void MouseFollowWithOffset()
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogWarning("PlayerController.Instance is null");
            return;
        }

        if (PlayerController.Instance.gameObject == null)
        {
            Debug.LogWarning("PlayerController gameObject is destroyed");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogWarning("Camera.main is null");
            return;
        }

        if (weaponCollider == null)
        {
            Debug.LogWarning("weaponCollider is null");
            return;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.Instance.transform.position);

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;


        if (mousePos.x < playerScreenPoint.x)
        {

            var currentWeapon = ActiveWeapon.Instance.CurrentActiveWeapon;
            if (currentWeapon != null)
            {
                currentWeapon.transform.rotation = Quaternion.Euler(0, -180, angle); 
            }
            //if (ActiveWeapon.Instance != null)
            //    ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, -180, angle);

            weaponCollider.transform.rotation = Quaternion.Euler(0, -180, 0);
        }
        else
        {
            var currentWeapon = ActiveWeapon.Instance.CurrentActiveWeapon;
            if (currentWeapon != null)
            {
                currentWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            //if (ActiveWeapon.Instance != null)
            //    ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);

            weaponCollider.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, Damagable
{
    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, JumpForce, smoothTime;
    [SerializeField] Item[] items;

    public HandGuns handGunsRight;
    public HandGuns handGunsLeft;
    public Transform skin;

    int itemIndex;
    int previousItemIndex = -1;

    float verticalLookRotation;
    bool grounded;

    Vector3 smoothMoveVelocity;

    Vector3 moveAmount;
    
    //Vida
    const float maxHealth = 100f;
    float currentHealth = maxHealth;
    [SerializeField] Image healthbarImg;
    [SerializeField] GameObject UI;
    [SerializeField] Image DamageImage;
    private float r, g, b, a;

    //Aim
    public GameObject aimImage;
    public GameObject sniperAimImage;
    public float defaultFov = 60;

    //Components
    Rigidbody rb;
    PhotonView PV;
    PlayerManager playerManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    void Start()
    {
        sniperAimImage.SetActive(false);
        aimImage.SetActive(true);
        aimImage.GetComponent<Image>().SetNativeSize();

        r = DamageImage.color.r;
        g = DamageImage.color.g;
        b = DamageImage.color.b;
        a = DamageImage.color.a;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (PV.IsMine)
        {
            EquipItem(0);
        }
        else{
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(UI);
        }
    }
    void Update()
    {
        skin.position = new Vector3(transform.position.x, transform.position.y-1, transform.position.z);
        if (!PV.IsMine) return;

        Look();
        Move();
        Jump();

        a = (100f - currentHealth)/100f;
        CanviarColor();

        if(Input.GetMouseButtonDown(1)){
            sniperAimImage.SetActive(true);
            aimImage.SetActive(false);
            items[itemIndex].itemGameObject.SetActive(false); 
            GetComponentInChildren<Camera>().fieldOfView = (defaultFov / 2);
        }
        if(Input.GetMouseButtonUp(1)){
            sniperAimImage.SetActive(false);
            aimImage.SetActive(true);
            items[itemIndex].itemGameObject.SetActive(true); 
            GetComponentInChildren<Camera>().fieldOfView = (defaultFov);
        }

        
        //Canviar d'arma amb numeros
        for(int i=0; i<items.Length; i++){
            if(Input.GetKeyDown((i+1).ToString())){
                EquipItem(i);
                break;
            }
        }
        //Canviar d'arma amb la roda del ratolí
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f){ //Amunt
            if(itemIndex >= items.Length -1 ){
                EquipItem(0);
            }
            else EquipItem(itemIndex+1);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f){ //Avall
            if(itemIndex <= 0 ){
                EquipItem(items.Length-1);
            }
            else EquipItem(itemIndex-1);
        }

        if(Input.GetMouseButtonDown(0)){
            items[itemIndex].Use();
        }

    }

   
    void Look()
    {      
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60f, 30f);

        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * JumpForce);
        }

    }

    void EquipItem(int _index){
        if(_index == previousItemIndex) return;
        
        itemIndex = _index;
        items[itemIndex].itemGameObject.SetActive(true); 

        handGunsRight.AssignarHandGuns( items[itemIndex].itemGameObject.transform.parent.transform.GetChild(0));
        handGunsLeft.AssignarHandGuns( items[itemIndex].itemGameObject.transform.parent.transform.GetChild(1));
        //aimImage.sprite = items[itemIndex].aim;
        //aimImage.SetNativeSize();


        if(previousItemIndex != -1){
            items[previousItemIndex].itemGameObject.SetActive(false); 
        }

        previousItemIndex = itemIndex;

        //Canviem les propietats del player i ho enviem
        if(PV.IsMine){
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    //Quan canvien les propietats del player... (mètode de rebre)
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps){
        if(changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner){
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;
        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    public void TakeDamage(float damage){
        PV.RPC(nameof(RPC_TakeDamage),PV.Owner,damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info){

        Debug.Log("took damage " + damage);
        currentHealth -= damage;

        //Barra de vida
        healthbarImg.fillAmount = currentHealth/maxHealth;
        

        if(currentHealth <= 0){
            Die();
            PlayerManager.Find(info.Sender).GetKill();
        }
    }

    void Die(){
        playerManager.Die();
    }

    private void CanviarColor(){
        Color c = new Color(r,g,b,a);
        DamageImage.color = c;
    }
}

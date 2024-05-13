using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AnimatorManager : MonoBehaviour
{
    //Animations
    public bool running;
    public bool walking;

    //Moviment
    public float velocity = 0.0f;
    public float acceleration = 0.8f;
    public float deceleration = 0.6f;
    int VelocityAnimator, MovingAnimator;

    //Components
    Animator anim;
    public PhotonView PV;

    void Start(){
        anim = GetComponent<Animator>();
        VelocityAnimator = Animator.StringToHash("Velocity");
        MovingAnimator = Animator.StringToHash("Moving");
    }

    void Update(){
        GetBools();

        if(running && velocity < 1.0f){
            velocity += Time.deltaTime * acceleration;
        }
        
        if(!running && velocity > 0.0f){
            velocity -= Time.deltaTime * deceleration;
        }

        if(!running && velocity < 0.0f){
            velocity = 0.0f;
        }

        if(PV.IsMine){
            anim.SetFloat(VelocityAnimator, velocity);
            anim.SetBool(MovingAnimator, walking || running);
        }
    }

    void GetBools(){
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        if(moveDir != new Vector3(0,0,0)){
            walking = true;
        }
        else walking = false;

        if(Input.GetKey(KeyCode.LeftShift) && walking){
            running = true;
        }
        else running = false;
    }
}

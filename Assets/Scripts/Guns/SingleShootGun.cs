using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleShootGun : Gun
{
    [SerializeField] Camera cam;
    [SerializeField] GameObject hitMarker;

    void Start(){
        hitMarker.SetActive(false);
    }

    public override void Use(){
        Shoot();
    }

    void Shoot(){
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if(Physics.Raycast(ray, out RaycastHit hit)){
            if(hit.collider.gameObject.GetComponent<Damagable>() != null){
                hit.collider.gameObject.GetComponent<Damagable>().TakeDamage(((GunInfo)itemInfo).damage);
                hitMarker.SetActive(true);
                Invoke("DisableHitMarker", 0.05f);
            }
        }
    }

    void DisableHitMarker(){
        hitMarker.SetActive(false);
    }
}

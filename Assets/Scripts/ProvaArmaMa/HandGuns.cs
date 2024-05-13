using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class HandGuns : MonoBehaviour
{
    public TwoBoneIKConstraint hand;
    public RigBuilder rigBuilder;

    public void AssignarHandGuns(Transform _hand){
        hand.data.target = _hand.transform;
        rigBuilder.Build();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;

public class PlayerCreature : PlayerCreatureBehavior
{
    Animator anm;
    private void Start()
    {
       // disable motion 
       GetComponent<Creature>().networkObject = networkObject.IsOwner;
       anm = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!networkObject.IsOwner)
        {
            // Assign the position of this cube to the position sent on the network
            transform.position = networkObject.position;

            // Assign the rotation of this cube to the rotation sent on the network
            transform.rotation = networkObject.rotation;
            anm.SetBool("Jump", networkObject.jump);
            anm.SetInteger("Move", networkObject.move);
            anm.SetBool("Attack", networkObject.attack);
            anm.SetFloat("Turn", networkObject.turn);
           // anm.SetFloat("Pitch", networkObject.pitch);
            anm.SetInteger("Idle", networkObject.idle);
            anm.SetBool("OnGround", networkObject.onground);
            networkObject.jump = false;
        } else
        {
            // Since we are the owner, tell the network the updated position
            networkObject.position = transform.position;

            // Since we are the owner, tell the network the updated rotation
            networkObject.rotation = transform.rotation;

            // Since we are the owner, tell the network the updated ANIM states
            networkObject.move = anm.GetInteger("Move");
            networkObject.attack = anm.GetBool("Attack");
            networkObject.turn = anm.GetFloat("Turn");
           // networkObject.pitch = anm.GetFloat("Pitch");
            networkObject.idle = anm.GetInteger("Idle");
            networkObject.onground = anm.GetBool("OnGround");
        }
    }
}

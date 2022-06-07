using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Generated;

public class GameLogic : MonoBehaviour
{
    // Start is called before the first frame update
#if UNITY_STANDALONE_LINUX
    void Start()
    {

        PlayerCreatureBehavior go = NetworkManager.Instance.InstantiatePlayerCreature(0, transform.position);
        Manager manager = GetComponent<Manager>();
        manager.creaturesList.Add(go.gameObject);
        manager.CameraMode = 1;
    }
#endif
}

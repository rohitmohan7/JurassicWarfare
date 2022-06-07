using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Unity;
using BeardedManStudios.Forge.Networking.Generated;

public class GameLogic : MonoBehaviour
{
    // Start is called before the first frame update

    void Start()
    {
#if UNITY_STANDALONE_LINUX
        var consoleUI = new ConsoleTextLinux();

		Console.Init(consoleUI);

		Console.SetOpen(true);

        Console.Write("Hosting Server!");
#else
        PlayerCreatureBehavior go = NetworkManager.Instance.InstantiatePlayerCreature(0, transform.position);
        Manager manager = GetComponent<Manager>();
        manager.creaturesList.Add(go.gameObject);
        manager.CameraMode = 1;
#endif
    }

#if UNITY_STANDALONE_LINUX
	private void LateUpdate()
	{
		Console.ConsoleLateUpdate();
	}
    private void Update()
    {
        Console.ConsoleUpdate();
    }
#endif

}

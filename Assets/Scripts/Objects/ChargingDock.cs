using UnityEngine;

public class ChargingDock : MonoBehaviour
{
	[SerializeField] public GameObject playerPrefab;

	private void Start()
	{
		if (playerPrefab == null)
		{
			Debug.LogError("Player prefab is not assigned in Charging Dock.");
			return;
		}

		// Spawn player & parent it to the root to make sure that it stays within the scene (mainly for editor testing where the scene gets unloaded on play)
		Instantiate(playerPrefab, transform.position, transform.rotation, transform.parent.root);
	}
}

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

		Instantiate(playerPrefab, transform.position, transform.rotation);
	}
}

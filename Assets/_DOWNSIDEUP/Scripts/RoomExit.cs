using UnityEngine;

public class RoomExit : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            LevelsManager.Instance.LoadNextLevel();
            AudioManager.Instance.PlayFootsteps = false;
            AudioManager.Instance.PlaySound(Sound.levelCompleted);
            other.gameObject.SetActive(false);
        }
    }
}

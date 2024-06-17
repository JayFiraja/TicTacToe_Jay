using UnityEngine;

public class UI_PlayAudioOnClick : MonoBehaviour
{
    private GameManager _gameManager;
    [SerializeField, Tooltip("Plays the clip on click")]
    private AudioClip clipToPlay;

    /// <summary>
    /// Plays the given clip on AudioManager
    /// </summary>
    public void PlayUIAudioClip()
    {
        if (_gameManager == null)
        {
            _gameManager = GameManager.S;
        }
        _gameManager.AudioManager.PlayUISFX(clipToPlay);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple frame-based animator for StoneChip prefabs.
/// Plays a short sequence of sprites at a fixed frame rate.
/// Designed for spawn (add) animation, with an optional click animation API you can call before removal.
/// </summary>
[RequireComponent(typeof(Image))]
public class StoneChipAnimator : MonoBehaviour
{
    [Header("Frames")]
    [Tooltip("Frames to play once when the chip is spawned (added). Leave empty to skip.")]
    public Sprite[] spawnFrames;

    [Tooltip("Frames to play once when the chip is clicked. Optional; call PlayClickOnce() before removal if you want a click animation.")]
    public Sprite[] clickFrames;

    [Header("Timing")] 
    [Tooltip("Frames per second for playback.")]
    public float frameRate = 12f;

    [Tooltip("If true, plays the spawn animation automatically in OnEnable.")]
    public bool playOnSpawn = true;

    [Header("Idle")]
    [Tooltip("Sprite to display after the animation finishes. If null, the last frame remains.")]
    public Sprite idleSprite;

    private Image _img;
    private Coroutine _playing;

    private void Awake()
    {
        _img = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (playOnSpawn && spawnFrames != null && spawnFrames.Length > 0)
        {
            PlayFramesOnce(spawnFrames);
        }
        else if (idleSprite != null)
        {
            _img.sprite = idleSprite;
        }
    }

    /// <summary>
    /// Call this (optionally) before removing the chip if you want a short click animation.
    /// You can yield the coroutine externally, or pass a callback to run when done.
    /// </summary>
    public void PlayClickOnce(System.Action onComplete = null)
    {
        if (clickFrames == null || clickFrames.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }
        PlayFramesOnce(clickFrames, onComplete);
    }

    private void PlayFramesOnce(Sprite[] frames, System.Action onComplete = null)
    {
        if (_playing != null) StopCoroutine(_playing);
        _playing = StartCoroutine(CoPlayFramesOnce(frames, onComplete));
    }

    private IEnumerator CoPlayFramesOnce(Sprite[] frames, System.Action onComplete)
    {
        if (frames == null || frames.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        float dt = 1f / Mathf.Max(1f, frameRate);
        for (int i = 0; i < frames.Length; i++)
        {
            _img.sprite = frames[i];
            yield return new WaitForSeconds(dt);
        }

        if (idleSprite != null)
            _img.sprite = idleSprite;

        onComplete?.Invoke();
        _playing = null;
    }
}
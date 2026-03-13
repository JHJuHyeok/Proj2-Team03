using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class TemporarySoundPlayer : MonoBehaviour
{
    private AudioSource _audioSource;
    public string ClipName { get; private set; }

    private void Awake()
    {
        // 재생시킬 오디오 소스 참조
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 사운드 초기화
    /// </summary>
    /// <param name="clip">초기화시킬 오디오 클립</param>
    public void InitSound(AudioClip clip)
    {
        ClipName = clip.name;
        _audioSource.clip = clip;
    }

    public void PlayAudio(AudioMixerGroup group, float delay, bool isLoop)
    {
        _audioSource.outputAudioMixerGroup = group;
        _audioSource.loop = isLoop;
        _audioSource.playOnAwake = false;

        _audioSource.PlayDelayed(delay);

        // 루프가 아니면 재생이 끝나고 풀로 반환
        if (!isLoop)
        {
            StartCoroutine(ReturnToPoolAfterPlayback(_audioSource.clip.length));
        }
    }

    public void StopAndReturn()
    {
        _audioSource.Stop();
        StopAllCoroutines();
        PoolManager.Instance.ReturnPool(this);
    }

    private IEnumerator ReturnToPoolAfterPlayback(float duration)
    {
        yield return new WaitForSeconds(duration);
        PoolManager.Instance.ReturnPool(this);
    }
}

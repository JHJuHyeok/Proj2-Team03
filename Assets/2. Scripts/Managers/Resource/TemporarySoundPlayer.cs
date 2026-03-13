using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class TemporarySoundPlayer : MonoBehaviour
{
    private AudioSource _audioSource;
    public string ClipName { get; private set; }

    private void Awake()
    {
        // �����ų ����� �ҽ� ����
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// ���� �ʱ�ȭ
    /// </summary>
    /// <param name="clip">�ʱ�ȭ��ų ����� Ŭ��</param>
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

        // ������ �ƴϸ� ����� ������ Ǯ�� ��ȯ
        if (!isLoop)
        {
            StartCoroutine(ReturnToPoolAfterPlayback(_audioSource.clip.length + delay));
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

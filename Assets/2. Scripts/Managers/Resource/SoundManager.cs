using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// 오디오 믹서 그룹명
public enum SoundType
{
    bgm,
    effect
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("오디오 설정")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private TemporarySoundPlayer soundPlayerPrefab;
    [SerializeField] private int initialPoolCount = 10;

    private float mCurrentVolume;

    // 오디오 클립 검색 딕셔너리
    private Dictionary<string, AudioClip> clipDict;
    // 루프 사운드 관리를 위한 리스트
    private List<TemporarySoundPlayer> activeLoopSounds = new();

    private void Start()
    {
        // 딕셔너리에 오디오 클립 등록
        clipDict = new Dictionary<string, AudioClip>();
        foreach (AudioClip clip in audioClips)
        {
            if (!clipDict.ContainsKey(clip.name))
                clipDict.Add(clip.name, clip);
        }

        // 지정 갯수만큼 풀 생성
        if (soundPlayerPrefab != null)
        {
            GameObject poolParent = new GameObject("SoundPool");
            PoolManager.Instance.CreatePool(soundPlayerPrefab, initialPoolCount, poolParent.transform);
        }
    }

    /// <summary>
    /// 오디오 이름 기반으로 오디오 클립 호출
    /// </summary>
    /// <param name="clipName"> 오디오 이름 </param>
    /// <returns> 이름과 일치하는 오디오 클립 </returns>
    private AudioClip GetClip(string clipName)
    {
        if (clipDict.TryGetValue(clipName, out AudioClip clip))
            return clip;

        return null;
    }

    /// <summary>
    /// 루프 사운드 중지
    /// </summary>
    /// <param name="clipName"> 오디오 클립 이름 </param>
    public void StopLoopSound(string clipName)
    {
        for (int i = activeLoopSounds.Count - 1; i >= 0; i--)
        {
            if (activeLoopSounds[i].ClipName == clipName)
            {
                var player = activeLoopSounds[i];
                activeLoopSounds.RemoveAt(i);
                player.StopAndReturn();
                return;
            }
        }
    }

    public void PlaySound(string clipName, float delay = 0f, bool isLoop = false, SoundType type = SoundType.effect)
    {
        AudioClip clip = GetClip(clipName);
        if (clip == null) return;

        // 풀에서 객체 가져오기
        TemporarySoundPlayer soundPlayer = PoolManager.Instance.GetFromPool(soundPlayerPrefab);
        if (soundPlayer == null) return;

        soundPlayer.InitSound(clip);

        // 믹서 그룹 할당 및 재생
        var mixerGroups = audioMixer.FindMatchingGroups(type.ToString());
        AudioMixerGroup targetGroup = mixerGroups.Length > 0 ? mixerGroups[0] : null;

        soundPlayer.PlayAudio(targetGroup, delay, isLoop);

        // 루프면 사운드 저장
        if (isLoop) { activeLoopSounds.Add(soundPlayer); }
    }

    /// <summary>
    /// 사운드 볼륨을 저장된 수치로 초기화
    /// </summary>
    /// <param name="bgm"> bgm 볼륨 수치 </param>
    /// <param name="effect"> 이펙트 볼륨 수치 </param>
    public void InitVolumes(float bgm, float effect)
    {
        SetVolume(SoundType.bgm, bgm);
        SetVolume(SoundType.effect, effect);
    }

    /// <summary>
    /// 볼륨 조절 함수
    /// </summary>
    /// <param name="type"> 사운드 타입 </param>
    /// <param name="value"> 설정 볼륨 </param>
    public void SetVolume(SoundType type, float value)
    {
        audioMixer.SetFloat(type.ToString(), value);
    }

    ///// <summary>
    ///// 옵션 변경 이벤트 함수
    ///// </summary>
    ///// <param name="type"></param>
    ///// <param name="value"></param>
    //private void OptionToggleEvent(OptionToggleControl.ToggleOptionType type, bool value)
    //{
    //    if (type == OptionToggleControl.ToggleOptionType.SFX)
    //    {
    //        if (value == true)
    //        {
    //            SetVolume(SoundType.effect, -20.0f);
    //        }
    //        if (value == false)
    //        {
    //            SetVolume(SoundType.effect, -80.0f);
    //        }
    //    }
    //    else if (type == OptionToggleControl.ToggleOptionType.BGM)
    //    {
    //        if (value == true)
    //        {
    //            SetVolume(SoundType.bgm, -20.0f);
    //        }
    //        if (value == false)
    //        {
    //            SetVolume(SoundType.bgm, -80.0f);
    //        }
    //    }
    //}
}

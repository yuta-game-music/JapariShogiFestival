using JSF.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace JSF.SE
{
    public class SEController : MonoBehaviour
    {
        public AudioMixerGroup SEMixer;

        public AudioClip ClickClip;
        public AudioClip StartClip;
        public void Start()
        {
            Util.SetSESource(this);
        }
        public void PlaySE(SEType SEType)
        {
            switch (SEType)
            {
                case SEType.Click:
                    PlaySE(ClickClip);
                    break;
                case SEType.Start:
                    PlaySE(StartClip);
                    break;
                default:
                    Debug.LogError("Unknown SEType: "+SEType);
                    break;
            }
        }
        public void PlaySE(AudioClip AudioClip)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = AudioClip;
            source.loop = false;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = SEMixer;
            source.volume = GlobalVariable.SEVolume;
            source.Play();
            StartCoroutine(CheckOpenSE(source));
        }

        private static IEnumerator CheckOpenSE(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            Destroy(source);
        }
    }

    public enum SEType
    {
        Click, Start
    }
}
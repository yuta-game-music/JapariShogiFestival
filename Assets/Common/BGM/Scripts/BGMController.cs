using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSF.Common;

namespace JSF.BGM
{
    public class BGMController : MonoBehaviour
    {
        public AudioSource AudioSource;

        public int LoopEndSamples;
        public int LoopLengthSamples;

        private void Start()
        {
            Util.SetBGMSource(AudioSource);
        }

        private void Update()
        {
            if (AudioSource)
            {
                AudioSource.volume = GlobalVariable.BGMVolume;
                if (AudioSource.timeSamples >= LoopEndSamples)
                {
                    AudioSource.timeSamples -= LoopLengthSamples;
                }
            }
        }
    }

}
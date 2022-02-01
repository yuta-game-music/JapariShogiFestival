using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSF.BGM
{
    public class BGMLoopController : MonoBehaviour
    {
        public AudioSource AudioSource;

        public int LoopEndSamples;
        public int LoopLengthSamples;

        private void Update()
        {
            if(AudioSource.timeSamples >= LoopEndSamples)
            {
                AudioSource.timeSamples -= LoopLengthSamples;
            }
        }
    }

}
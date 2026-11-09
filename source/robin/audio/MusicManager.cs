using Attribute;
using Audio.Scriptable;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Audio.Component
{
    public class MusicManager : MonoBehaviour
    {
        /// <summary>
        /// Overall Music Manager. Starts Main Music Event and can control BPM changes. todo: start other fmod actions and events from here
        /// </summary>
        
        [Expandable][SerializeField] private BeatData _beatData;
        [Expandable][SerializeField] private TrackData _trackData;
        //public EventReference musicEvent;   // FMODEvent Reference
        private EventInstance _musicInstance;
        private BeatSystem _bs;

        private int _toggleValue;
        private float _cutOffValue;
        

        private void Awake()
        {
            _bs = GetComponent<BeatSystem>();
            _musicInstance = RuntimeManager.CreateInstance(_trackData.TrackEvent);
            _musicInstance.start();
            _bs.AssignBeatEvent(_musicInstance);
            
            _beatData.Bpm = _trackData.BPM;
            _beatData.Beat = 60f / _trackData.BPM;
            
            ChangeBpm(0);
            _cutOffValue = 0;

        }

        private void Update()
        {
            ChangeBpm(_beatData.FmodBpmChange);
        }

        private void ChangeBpm(float value)
        {
            _musicInstance.setParameterByName("fmodBpmChange", value);
        }
        
        public void SetFmodParameter(float value, string parameterName)
        {
            _musicInstance.setParameterByName(parameterName, value);
        }
        
        private void SetMusicSection(float value) { SetFmodParameter(value, "SectionToggle"); }

        public void ReduceFilter(float value)
        {
            _cutOffValue += value;
            _cutOffValue = Mathf.Clamp(_cutOffValue, 0, 1);
            SetFmodParameter(_cutOffValue, "BassCutOffFilter");
        }

        public void SetPianoIn1(float value) {SetFmodParameter(value, "PianoIn1");}
        public void SetPianoIn2(float value) {SetFmodParameter(value, "PianoIn2");}
        public void SetPiano(float value) {SetFmodParameter(value, "Piano");}
        public void SetBass(float value) {SetFmodParameter(value, "Bass");}
        public void SetPercussion(float value) {SetFmodParameter(value, "Percussion");}
        public void SetSnare(float value) {SetFmodParameter(value, "Snare");}
        public void SetKick(float value) {SetFmodParameter(value, "Kick");}
        public void SetPluck(float value) {SetFmodParameter(value, "Pluck");}
        public void SetString(float value) {SetFmodParameter(value, "String");}
        public void SetSubBass(float value) {SetFmodParameter(value, "SubBass");}
        public void SetLimePad(float value) {SetFmodParameter(value, "LimePad");}
        public void SetBrass(float value) {SetFmodParameter(value, "Brass");}

        public void SetMusicSection1() { SetMusicSection(0); SetBass(1); }

        public void SetMusicSection2()
        {
            SetMusicSection(1); ReduceFilter(1); 
            SetBass(1);
        }

        public void SetMusicSection3()
        {
            SetMusicSection(1); ReduceFilter(1); 
            SetBass(1); SetLimePad(1); SetPianoIn1(0); SetPianoIn2(0); SetPiano(1);
        }

        public void SetMusicSection4()
        {
            SetMusicSection(1); ReduceFilter(1); 
            SetBass(0); SetLimePad(0); SetPiano(1);
        }

        public void SetMusicSection5()
        {
            SetMusicSection(1); ReduceFilter(1); 
            SetBass(0); SetString(0); SetSubBass(1); SetPluck(1); 
            SetKick(1); SetSnare(1); SetPercussion(1);
        }

        public void SetMusicSection6()
        {
            SetMusicSection(1); ReduceFilter(1); 
            SetBass(0); SetString(1); SetSubBass(0); SetPluck(1); SetLimePad(1); SetPianoIn1(1); SetPianoIn2(1); SetPiano(0); 
            SetKick(0); SetSnare(1); SetPercussion(0);
        }

        public void SetMusicSection7()
        {
            SetMusicSection(1); ReduceFilter(1); 
            SetBass(1); SetSubBass(1); SetBrass(1); SetString(1); SetPluck(1); SetLimePad(1);
            SetPianoIn1(0); SetPianoIn2(0); SetPiano(1);
            SetKick(1); SetSnare(1); SetPercussion(1);
        }
        

        private void OnDestroy()
        {
            _musicInstance.stop(STOP_MODE.ALLOWFADEOUT);
            _musicInstance.release();
            _bs.StopAndClear(_musicInstance);
        }
    }
}

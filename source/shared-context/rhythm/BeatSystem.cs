using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;
using Audio.Scriptable;
using Core.Channel;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using Utility.Extensions;
using Utility.Smoothing;
using Debug = UnityEngine.Debug;

namespace Audio.Component
{
    internal class BeatSystem : MonoBehaviour
    {
        /// <summary>
        /// Script for FMOD Studio Callback Events.
        /// Detects Beats and Markers from used FMOD Studio project.
        /// Invokes Delegates BeatChangedEvent and MarkerChangedEvent.
        /// </summary>
        
        private int _lastBeat = 0;
        private string _lastMarker = null;

        [SerializeField] private BeatSystemChannel _beatSystemChannel;

        [StructLayout(LayoutKind.Sequential)] //for better memory use
        private class TimelineInfo
        {
            public int currentMusicBeat = 0;
            public StringWrapper lastMarker = new StringWrapper();
        }

        
        private TimelineInfo _timelineInfo; //store timelineInfo data -> maybe need to be public
        private GCHandle _timelineHandle; //GCHandle -> gain access to objects in not administered memory

        private EVENT_CALLBACK _beatCallback;
        

        private static float _beat;
        private static string _marker;

        private static float _bpm;
        [SerializeField] private BeatData beatData;
        private int _comboCounter;

        private char _lastCharacter;
        private static bool _hasFirstMarker;

        public void AssignBeatEvent(EventInstance instance) //AssignBeatEvent() allows to assign an instance that was actually created and started somewhere else to this beat system
        {
            _timelineInfo = new TimelineInfo(); //create Instance of timeLineInfo
            _timelineHandle = GCHandle.Alloc(_timelineInfo, GCHandleType.Pinned); //create Instance of GCHandle, ignore garbage collection, tool for accessing callbacks
            _beatCallback = new EVENT_CALLBACK(BeatEventCallback); //create Instance of BeatEventCallback
            
            instance.setUserData(GCHandle.ToIntPtr(_timelineHandle)); //assign the handle to the music instance
            instance.setCallback(_beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT | EVENT_CALLBACK_TYPE.TIMELINE_MARKER); //search for beats and markers
        }

        public void StopAndClear(EventInstance instance) //Stop the created instance in AssignBeatEvent()
        {
            instance.setUserData(IntPtr.Zero); //set pointer back to zero
            instance.stop(STOP_MODE.ALLOWFADEOUT); 
            instance.release(); //release instance
            _timelineHandle.Free(); //free timelineHandle
        }

        [MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private static RESULT BeatEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr) //takes a pointer to the event instance and a pointer to the parameter data
        {
            //IntPtr is a datatype used for pointers
            EventInstance instance = new EventInstance(instancePtr); //create EventInstance through instancePtr
            RESULT result = instance.getUserData(out IntPtr timelineInfoPtr); //result determines if it is the right result
            
            if (result != RESULT.OK) //check if result is not ok 
            {
                Debug.LogError("Timeline Callback error: " + result); //show error in console
            }
            else if (timelineInfoPtr != IntPtr.Zero) //check if timelineInfoPtr points on something
            {
                GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr); 
                TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target; //object we store beat and marker details

                switch (type) //check for EVENT_CALLBACK_TYPE case
                {
                    case EVENT_CALLBACK_TYPE.TIMELINE_BEAT: //if its a timeline_beat
                    {
                        TIMELINE_BEAT_PROPERTIES parameter = (TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_BEAT_PROPERTIES)); //parameter stores beat properties
                        timelineInfo.currentMusicBeat = parameter.beat; //save parameter beat to the timelineInfo attribute currentMusicBeat
                        _beat = timelineInfo.currentMusicBeat;
                    }
                        break;
                    case EVENT_CALLBACK_TYPE.TIMELINE_MARKER: //if its a timeline_marker
                    {
                        TIMELINE_MARKER_PROPERTIES parameter = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name; //save parameter name to the timelineInfo attribute lastMarker
                        _marker = timelineInfo.lastMarker;
                    }
                        break;
                }
            }
            //Debug.Log("beat changed");
            return RESULT.OK; //return ok result
        }

        private void Start()
        {
            _lastMarker ??= "";
        }

        private void Update()
        {
            if (_lastMarker != _timelineInfo.lastMarker)
            {
                _lastMarker = _timelineInfo.lastMarker;
                _beatSystemChannel.AnnounceMarkerChange(_lastMarker);
                
                _lastCharacter = _lastMarker[^1];
                switch (_lastCharacter)
                {
                        case 'A':
                        {   
                            Debug.Log("A");
                            _beatSystemChannel.AnnounceMarkerNoteEvent();
                            break;
                        }
                }
            }
            //Debug.Log($"{_lastMarker} und {_timelineInfo.lastMarker}");
            
            //
            // if (_lastMarker != _timelineInfo.lastMarker)
            // {
            //     _lastMarker = _timelineInfo.lastMarker;
            //     _beatSystemChannel.AnnounceMarkerChange(_lastMarker);
            //
            //     if (_lastMarker != null) _lastCharacter = _lastMarker[^1];
            //     switch (_lastCharacter)
            //     {
            //             case 'A':
            //             {
            //                 _beatSystemChannel.AnnounceMarkerNoteEvent();
            //                 break;
            //             }
            //     }
            //         
            //     
            // }
            
            if (_lastBeat == _timelineInfo.currentMusicBeat) return;
            _lastBeat = _timelineInfo.currentMusicBeat;
            _beatSystemChannel.AnnounceBeatChange(_lastBeat);
            
            // SyncCounter on BeatChangedEvent
            StartCoroutine(SyncCounter()); 
            
            // Look for Bpm and Data after MarkerChangedEvent
            // if ('M' != _lastMarker[^1]) return;
            // if (!float.TryParse(_lastMarker, out float result)) return;
            // Debug.Log("Hey" + result);
            // beatData.Bpm = (int) result;
            // _beat = 60 / result;
            // beatData.Beat = _beat;
        }
        
        private IEnumerator SyncCounter()
        {
            float t = 0f;
            float timePercentage = 0;
            while (timePercentage < 1)
            {
                t += Time.deltaTime;
                timePercentage = t / _beat;
                beatData.SyncTime = timePercentage;
                beatData.BeatTime = t;

                yield return null;
            }

            // this.DoLerpRoutine(
            //     duration: _beat,
            //     routineAction: t =>
            //     {
            //         beatData.SyncTime = t;
            //         beatData.BeatTime = Lerp.Value(0, _beat, t, Easing.Linear);
            //
            //         // Debug.Log($"Sync Time: {beatData.SyncTime}");
            //         // Debug.Log($"Beat Time: {beatData.BeatTime}");
            //     });
        }
    }
}

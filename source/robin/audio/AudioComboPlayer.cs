using System;
using System.Collections;
using System.Collections.Generic;
using Audio.Scriptable;
using Core.Enum;
using UnityEngine;
using UnityEngine.Events;

public class AudioComboPlayer : MonoBehaviour
{
    [SerializeField] private BeatData _beatData;

    [SerializeField] private UnityEvent _playSnap;
    [SerializeField] private UnityEvent _playClap;
    [SerializeField] private UnityEvent _playSnare;

    public void PlayAudio()
    {
        switch (_beatData.ComboMultiplier)
        {
            case ComboMultiplier.X1:
                _playSnap.Invoke();
                break;
            case ComboMultiplier.X2:
                _playClap.Invoke();
                break;
            case ComboMultiplier.X4:
                _playClap.Invoke();
                break;
            case ComboMultiplier.X8:
                _playSnare.Invoke();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

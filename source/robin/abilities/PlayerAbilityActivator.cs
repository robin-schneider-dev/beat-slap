using System;
using System.Collections;
using AbilitySystem;
using Audio;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAbilityActivator : MonoBehaviour
    {
    
        /// <summary>
        /// Script on Player. Setup from PlayCore.
        /// </summary>
        
        private AbilityData[] _abilities;
        private float _coolDownTime;
        private float _activeTime;
        private bool[] _abilityActive;

        private PlayerCore _playerCore;
        
        [SerializeField] private BeatSystem beatSystem;
        private BeatData _beatData;

        private double _hitRate;


        private SpriteRenderer _spriteRenderer;
        private int _comboCounter;
        private int _failCounter;
        
        public Action OnSuccess;
        

        public void Setup(PlayerCore playerCore)
        {
            _playerCore = playerCore;
            _abilities = _playerCore.PlayerData.abilities;
            _beatData = _playerCore.BeatData;
            _abilityActive = new bool[] {false, false, false, false};

            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            beatSystem.BeatChangedEvent += Sync;
        }

        private void OnDisable()
        {
            beatSystem.BeatChangedEvent -= Sync;
        }
        
        public void OnAbility1(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.performed) return;
            if (_abilityActive[0]) return;
            CalculateHitRate(0);
            if (_abilities[0].syncBeat) return;
            StartCoroutine(UseAbility(0));
        }
        
        public void OnAbility2(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.performed) return;
            if (_abilityActive[1]) return;
            CalculateHitRate(1);
            if (_abilities[1].syncBeat) return;
            StartCoroutine(UseAbility(1));
        }
        
        public void OnAbility3(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.performed) return;
            if (_abilityActive[2]) return;
            CalculateHitRate(2);
            if (_abilities[2].syncBeat) return;
            StartCoroutine(UseAbility(2));
        }
        
        public void OnAbility4(InputAction.CallbackContext callbackContext)
        {
            if (!callbackContext.performed) return;
            if (_abilityActive[3]) return;
            CalculateHitRate(3);
            if (_abilities[3].syncBeat) return;
            StartCoroutine(UseAbility(3));
        }

        private IEnumerator UseAbility(int i)
        {
            _abilityActive[i] = true;
            _abilities[i].Activate(gameObject, _beatData.successRate, _beatData.combo, _beatData.success);
            yield return new WaitForSeconds(_abilities[i].activeTime);
            _abilities[i].BeginCooldown(gameObject);
            yield return new WaitForSeconds(_abilities[i].cooldownTime);
            _abilityActive[i] = false;
            yield return null;
        }
        
        private void Sync(int currentBeat)
        {
            _beatData.successNow = false;
            StartCoroutine(HitScanner(_playerCore.PlayerData.hitAccurationRate));
        }

        private IEnumerator HitScanner(float hitAcc)
        {
            var timeElapsed = 0f;
            var timePercentage = 0f;

            while (timePercentage < 1)
            {
                timeElapsed += Time.deltaTime;
                timePercentage = timeElapsed / _beatData.beat;
                
                if (timePercentage <= 0 + hitAcc)
                {
                    _hitRate = timePercentage;
                }
                else if (timePercentage >= 1 - hitAcc)
                {
                    
                    if (timePercentage > 1)
                    {
                        timePercentage = 1;
                    }
                    _hitRate = 1 - timePercentage;
                }
                else
                {
                    _hitRate = timePercentage;
                }
                yield return null;
            }
        }

        private void CalculateHitRate(int i)
        {
            if (_hitRate < _playerCore.PlayerData.hitAccurationRate / 3)
            {
                Debug.Log("Perfect " + _hitRate);
                _spriteRenderer.color = Color.green;
                HitSuccess();
                if (_abilities[i].syncBeat)
                {
                    StartCoroutine(UseAbility(i));
                }
            }
            else if (_hitRate < (_playerCore.PlayerData.hitAccurationRate / 3) * 2)
            {
                Debug.Log("Not Bad " + _hitRate); 
                _spriteRenderer.color = Color.cyan;
                HitSuccess();
                if (_abilities[i].syncBeat)
                {
                    StartCoroutine(UseAbility(i));
                }
            }
            else if (_hitRate < _playerCore.PlayerData.hitAccurationRate)
            {
                Debug.Log("At least u hit " + _hitRate); 
                _spriteRenderer.color = Color.blue;
                HitSuccess();
                if (_abilities[i].syncBeat)
                {
                    StartCoroutine(UseAbility(i));
                }
            }
            else
            {
                HitFail();
                _spriteRenderer.color = Color.red;
                Debug.Log("U suc! " + _hitRate);
            }
        }

        private void HitSuccess()
        {
            ++_comboCounter;
            _beatData.combo = _comboCounter;
            _beatData.success = true;
            _beatData.successNow = true;
            _failCounter = 0;
            OnSuccess?.Invoke();
        }

        private void HitFail()
        {
            _beatData.success = false;
            _comboCounter = 0;
            _beatData.combo = _comboCounter;
            _beatData.fail = _failCounter;
        }
    }
}

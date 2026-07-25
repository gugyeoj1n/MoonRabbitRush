using System;
using System.Collections;
using UnityEngine;

namespace MoonRabbitRush.Combat
{
    public sealed class TimedEffect : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _duration = 0.5f;

        private Coroutine _releaseRoutine;
        private bool _isReleased;

        public event Action<TimedEffect> Released;

        private void OnEnable()
        {
            _isReleased = false;
            _releaseRoutine = StartCoroutine(ReleaseAfterDuration());
        }

        private void OnDisable()
        {
            if (_releaseRoutine != null)
            {
                StopCoroutine(_releaseRoutine);
                _releaseRoutine = null;
            }
        }

        public void Release()
        {
            if (_isReleased)
            {
                return;
            }

            _isReleased = true;

            if (Released != null)
            {
                Released.Invoke(this);
                return;
            }

            Destroy(gameObject);
        }

        private IEnumerator ReleaseAfterDuration()
        {
            yield return new WaitForSeconds(_duration);
            _releaseRoutine = null;
            Release();
        }
    }
}

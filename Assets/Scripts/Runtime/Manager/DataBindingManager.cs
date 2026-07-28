using System;
using System.Collections.Generic;
using UnityEngine;
using R3;
using UnityEngine.UI;
using TMPro;

namespace MoonRabbitRush
{
    public enum Property
    {
        None = 0,
        PlayerHealth = 1,
        PlayerExperience = 2,
        PlayerLevel = 3,
        PlayerMaxHealth = 4,
    }
    /// <summary>
    /// 간단한 데이터 바인딩 도우미와 예제.
    /// 외부 R3 패키지가 없더라도 동작하는 로컬 ObservableProperty 구현과
    /// UnityEngine.UI.Text에 바인딩하는 메서드를 제공합니다.
    /// R3 패키지를 사용하려면 아래 주석의 R3 예시를 참고해 대체하면 됩니다.
    /// </summary>
    public static class DataBindingManager
    {
        

        /// <summary>
        /// Property(열거형) 기반으로 UnityEngine.UI.Text에 바인딩합니다. GameObject 생명주기에 맞춰 구독을 자동 해제합니다.
        /// </summary>
        public static void BindText(Property type, Text ui, bool setInitial = true)
        {
            if (ui == null) return;
            ReactiveProperty<int> property = null;
            if (!_intProperties.TryGetValue(type, out property))
            {
                property = new ReactiveProperty<int>(0);
                _intProperties[type] = property;
            }

            if (setInitial && property != null) 
                ui.text = property.Value.ToString();

            property.Subscribe(v => 
            { 
                if (ui) 
                    ui.text = v.ToString(); 
            });
        }
        // R3의 IReactiveProperty<T> / ReactiveProperty<T>를 사용하는 바인딩 유틸
        // 이름으로 관리하는 정수형 ReactiveProperty 저장소
        static readonly Dictionary<Property, ReactiveProperty<int>> _intProperties = new Dictionary<Property, ReactiveProperty<int>>();

        /// <summary>
        /// 이름으로 ReactiveProperty<int>를 생성하거나 기존 항목의 값을 설정합니다.
        /// </summary>
        public static void Register(Property type, int initialValue)
        {
            if (_intProperties.TryGetValue(type, out var rp))
            {
                rp.Value = initialValue;
            }
            else
            {
                var newRp = new ReactiveProperty<int>(initialValue);
                _intProperties[type] = newRp;
            }
        }

        /// <summary>
        /// 이름으로 등록된 ReactiveProperty<int>가 있으면 Dispose(가능하면)하고 제거합니다.
        /// </summary>
        public static bool UnRegister(Property type)
        {
            if (_intProperties.TryGetValue(type, out var rp))
            {
                _intProperties.Remove(type);
                try
                {
                    (rp as IDisposable)?.Dispose();
                }
                catch 
                {
                    Debug.LogError("Property Dispose Error!");
                }
                return true;
            }
            return false;
        }

        public static void BindText(Property type, TextMeshProUGUI ui)
        {
            // 기존의 간단 바인딩은 자동 해제를 지원하지 않으므로, 수명 관리가 포함된 오버로드로 위임합니다.
            BindText(type, ui, true);
        }

        /// <summary>
        /// 내부적으로 관리되는 ReactiveProperty를 시도 반환합니다.
        /// </summary>
        public static bool TryGetProperty(Property type, out ReactiveProperty<int> property)
        {
            return _intProperties.TryGetValue(type, out property);
        }

        /// <summary>
        /// 값의 증감(+/-) 용도로 사용합니다. 전달된 값은 현재 값에 더해집니다.
        /// 해당 프로퍼티가 없으면 전달된 값으로 새로 생성합니다.
        /// (절대값 설정이 필요하면 Register(...)를 사용하세요.)
        /// </summary>
        public static void AddValue(Property type, int delta)
        {
            if (_intProperties.TryGetValue(type, out var rp))
            {
                rp.Value = rp.Value + delta;
            }
        }

        /// <summary>
        /// 프로퍼티에 절대값을 설정합니다. 기존에 없으면 생성합니다.
        /// Register와 동일한 동작을 수행하는 편의 메서드입니다.
        /// </summary>
        public static void SetValue(Property type, int value)
        {
            if (_intProperties.TryGetValue(type, out var rp))
            {
                rp.Value = value;
            }
            else
            {
                var newRp = new ReactiveProperty<int>(value);
                _intProperties[type] = newRp;
            }
        }

        /// <summary>
        /// 이름으로 존재하는 프로퍼티에 대해서만 값을 설정합니다. 존재하지 않으면 false를 반환합니다.
        /// </summary>
        public static bool TrySetValue(Property type, int value)
        {
            if (_intProperties.TryGetValue(type, out var rp))
            {
                rp.Value = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 이름으로 등록된 값을 가져옵니다.
        /// </summary>
        public static bool TryGetValue(Property type, out int value)
        {
            value = default;
            if (TryGetProperty(type, out var p))
            {
                value = p.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 이름(열거형)으로 구독을 등록합니다. 해당 프로퍼티가 없으면 0으로 새로 생성합니다.
        /// 반환되는 IDisposable을 Dispose하면 구독이 해제됩니다.
        /// </summary>
        public static IDisposable Subscribe(Property type, Action<int> callback)
        {
            if (callback == null) return null;
            if (!_intProperties.TryGetValue(type, out var rp))
            {
                rp = new ReactiveProperty<int>(0);
                _intProperties[type] = rp;
            }
            return rp.Subscribe(callback);
        }

        /// <summary>
        /// Property(열거형) 기반으로 TextMeshProUGUI에 바인딩합니다. GameObject 생명주기에 맞춰 구독을 자동 해제합니다.
        /// </summary>
        public static void BindText(Property type, TextMeshProUGUI ui, bool setInitial = true)
        {
            if (ui == null) return;
            ReactiveProperty<int> property = null;

            if (!_intProperties.TryGetValue(type, out property))
            {
                // 자동 생성하지 않으면 바인딩만 하지 않음. 대신 생성하고 바인딩하도록 선택할 수 있습니다.
                property = new ReactiveProperty<int>(0);
                _intProperties[type] = property;
            }

            if (setInitial && property != null) ui.text = property.Value.ToString();

            property.Subscribe(v => { if (ui) ui.text = v.ToString(); });
        }

        public static void BindSliderRatio(Property current, Property max, Slider slider)
        {
            void Refresh()
            {
                TryGetValue(current, out var cur);
                TryGetValue(max, out var total);

                slider.value = total == 0 ? 0f : (float)cur / total;
            }

            Subscribe(current, _ => Refresh());
            Subscribe(max, _ => Refresh());

            Refresh();
        }

    }
}

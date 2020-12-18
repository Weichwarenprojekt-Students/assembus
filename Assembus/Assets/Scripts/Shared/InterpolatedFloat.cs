using System;
using UnityEngine;

namespace Shared
{
    public class InterpolatedFloat
    {
        
        /// <summary>
        ///     The power of the linear interpolation
        /// </summary>
        private float _power;

        /// <summary>
        ///     The actual value
        /// </summary>
        private float _value;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="value">The actual value</param>
        /// <param name="power">The power of the linear interpolation</param>
        public InterpolatedFloat(float value = 0f,float power = 3f)
        {
            _value = value;
            _power = power;
        }
        /// <summary>
        ///     Overloading == operator
        /// </summary>
        /// <param name="interpolatedFloat"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator ==(InterpolatedFloat interpolatedFloat, float value)
        {
            return interpolatedFloat != null && Mathf.Abs(interpolatedFloat.Value() - value) < 0.1f;
        }

        /// <summary>
        ///     Overloading != operator
        /// </summary>
        /// <param name="interpolatedFloat"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool operator !=(InterpolatedFloat interpolatedFloat, float value)
        {
            return !(interpolatedFloat == value);
        }

        /// <summary>
        ///     Interpolate the new value (with respect to the given delta time)
        /// </summary>
        /// <param name="newValue">The new value that shall be targeted</param>
        public void ToValue(float newValue)
        {
            _value += (newValue - _value) * _power * Time.deltaTime;
        }

        /// <summary>
        ///     Returns the current value of the interpolation
        /// </summary>
        /// <returns>The current value</returns>
        public float Value()
        {
            return _value;
        }
    }
}
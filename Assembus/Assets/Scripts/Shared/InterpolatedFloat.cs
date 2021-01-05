using UnityEngine;

namespace Shared
{
    public class InterpolatedFloat
    {
        /// <summary>
        ///     The power of the linear interpolation
        /// </summary>
        private readonly float _power;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="value">The actual value</param>
        /// <param name="power">The power of the linear interpolation</param>
        public InterpolatedFloat(float value = 0f, float power = 3f)
        {
            Value = value;
            _power = power;
        }

        /// <summary>
        ///     The actual value
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        ///     Checks if the interpolated float is at a certain value
        /// </summary>
        /// <param name="value">Value to compare to the interpolated float</param>
        /// <returns>True if the values are equal</returns>
        public bool IsAtValue(float value)
        {
            return Mathf.Abs(Value - value) < 0.1f;
        }


        /// <summary>
        ///     Interpolate the new value (with respect to the given delta time)
        /// </summary>
        /// <param name="newValue">The new value that shall be targeted</param>
        public void ToValue(float newValue)
        {
            Value += (newValue - Value) * _power * Time.deltaTime;
        }
    }
}
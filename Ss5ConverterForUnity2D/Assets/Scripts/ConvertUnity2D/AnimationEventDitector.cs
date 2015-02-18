using UnityEngine;
using System.Collections;

namespace ConvertUnity2D
{
    public class AnimationEventDitector : MonoBehaviour
    {
        public delegate void OnIntEvent(int n);
        public delegate void OnFloatEvent(float f);
        public delegate void OnStringEvent(string s);
        public delegate void OnObjectEvent(Object n);
        public OnIntEvent OnIntEventCheck = null;
        public OnFloatEvent OnFloatEventCheck = null;
        public OnStringEvent OnStringEventCheck = null;
        public OnObjectEvent OnObjectEventCheck = null;
        void IntEvent(int n)
        {
            if (OnIntEventCheck != null)
            {
                OnIntEventCheck(n);
            }
        }
        void FloatEvent(float f)
        {
            if (OnFloatEventCheck != null)
            {
                OnFloatEventCheck(f);
            }
        }
        void StringEvent(string s)
        {
            if (OnStringEventCheck != null)
            {
                OnStringEventCheck(s);
            }
        }
        void ObjectEvent(Object obj)
        {
            if (OnObjectEventCheck != null)
            {
                OnObjectEventCheck(obj);
            }
        }
    }
}
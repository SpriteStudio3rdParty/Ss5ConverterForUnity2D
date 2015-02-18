using UnityEngine;
using System.Collections;

namespace ConvertUnity2D
{
    [ExecuteInEditMode()]
    public class PartDitector : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        public float orderInLayer;
        public Vector3 localEulerAngles;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void LateUpdate()
        {
            SetLocalEulerAngles();
            SetOrderInLayer();
        }

        void SetLocalEulerAngles()
        {
            if (transform.localEulerAngles.GetHashCode() != localEulerAngles.GetHashCode())
            {
                transform.localEulerAngles = localEulerAngles;
            }
        }

        void SetOrderInLayer()
        {
            if (spriteRenderer == null)
            {
                return;
            }
        
            if ((int) orderInLayer != spriteRenderer.sortingOrder)
            {
                spriteRenderer.sortingOrder = (int) orderInLayer;
            }
        }
    }
}

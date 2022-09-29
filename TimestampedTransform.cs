using UnityEngine;

namespace Fp2Trainer
{
    public class TimestampedTransform
    {
        public float timestamp = 0f;
        public Vector2 position = Vector2.zero;
        public Vector2 rotation = Vector2.zero;

        public TimestampedTransform(float timestamp, Vector2 position, Vector2 rotation)
        {
            this.timestamp = timestamp;
            this.position = position;
            this.rotation = rotation;
        }
        
        public TimestampedTransform(float timestamp, Transform transform)
        {
            this.timestamp = timestamp;
            this.position = new Vector2(transform.position.x, transform.position.y);
            this.rotation = new Vector2(transform.rotation.x, transform.rotation.y);
        }

        public TimestampedTransform()
        {
            
        }
    }
}
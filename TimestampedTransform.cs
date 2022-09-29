using UnityEngine;

namespace Fp2Trainer
{
    public class TimestampedTransform
    {
        public float timestamp = 0f;
        public Vector2 position = Vector2.zero;
        public float angle = 0f;

        public TimestampedTransform(float timestamp, Vector2 position, float angle)
        {
            this.timestamp = timestamp;
            this.position = position;
            this.angle = angle;
        }
        
        // Do not use this.
        public TimestampedTransform(float timestamp, Transform transform)
        {
            this.timestamp = timestamp;
            this.position = new Vector2(transform.position.x, transform.position.y);
            //this.angle = new Vector2(transform.rotation.x, transform.rotation.y);

            this.angle = 0; //This is wrong.
        }

        public TimestampedTransform()
        {
            
        }
    }
}
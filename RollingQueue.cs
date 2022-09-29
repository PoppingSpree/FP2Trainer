using System.Collections.Generic;

namespace Fp2Trainer
{
    public class RollingQueue<T>
    {
        protected List<T> queueItemList;
        protected int maxLength = 30;

        public RollingQueue()
        {
            queueItemList = new List<T>();
            queueItemList.Capacity = this.maxLength;
            //TheType = typeof(T);
        }
        
        public RollingQueue(int maxLength)
        {
            queueItemList = new List<T>();
            this.maxLength = maxLength;
            queueItemList.Capacity = this.maxLength;
        }

        public T Add(T item)
        {
            T purgedItem = default(T); 
            while (queueItemList.Count >= maxLength && maxLength > 0)
            {
                purgedItem = queueItemList[0];
                queueItemList.RemoveAt(0);
            }
            queueItemList.Add(item);

            return purgedItem;
        }
        
        public T GetLatest()
        {
            T item = default(T);
            int count = queueItemList.Count;
            if (count > 0)
            {
                item = queueItemList[count - 1];
            }

            return item;
        }
        
        public T GetPrevious()
        {
            T item = default(T);
            int count = queueItemList.Count;
            if (count > 1)
            {
                item = queueItemList[count - 2];
            }
            else if (count > 0)
            {
                item = queueItemList[count - 1];
            }

            return item;
        }

        override public string ToString()
        {
            string str = "---{RollingQueue}---";
            foreach (var queueItem in queueItemList)
            {
                str += queueItem + "\n";
            }
            str += "---{RollingQueue}---";

            return str;
        }
    }
}
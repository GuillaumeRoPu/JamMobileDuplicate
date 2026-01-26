using UnityEngine;

namespace Frynity.Utils
{
    public static class FryUtilSystem
    {
        public static void Log(this object message)
        {
            Debug.Log(message.ToString());
        }

        public static int BitIndex(this int value)
        {
            int v = value;
            int i = 0;
            while (v > 1)
            {
                v /= 2;
                i++;
            }

            if (v < 1)
                i = 0;

            return i;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Gilzoide.SafeAreaLayout
{
    public class IgnoreSafeArea : MonoBehaviour, ILayoutIgnorer
    {
        public bool ignoreLayout => true;
    }
}

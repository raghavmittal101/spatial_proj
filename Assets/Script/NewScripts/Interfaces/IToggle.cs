using UnityEngine;

namespace ThreeDGeneration.UI
{
    public interface IToggle
    {
        void Select();

        void UnSelect();

        bool IsSelected();
    }
}
using UnityEngine;

namespace HandPhysicsToolkit.Helpers.Interfaces
{
    public interface BaseInteractor
    {
        GameObject GetGameObject();
        bool GetIsLeftHand();

        bool GetIsHandTracking();
    }
}
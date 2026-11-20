using UnityEngine;

namespace NeonSurge
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class TrackCheckpoint : MonoBehaviour
    {
        private RaceManager race;
        private int index;

        public int Index { get { return index; } }

        public void Initialize(int checkpointIndex, RaceManager manager)
        {
            index = checkpointIndex;
            race = manager;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!race || !race.CanRegisterCheckpoints) return;
            var car = other.GetComponentInParent<NeonCarController>();
            if (car != null) race.RegisterCheckpoint(index);
        }
    }
}

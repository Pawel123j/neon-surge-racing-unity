using UnityEngine;

namespace NeonSurge
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class NeonCarController : MonoBehaviour
    {
        private const float MaximumSpeed = 20f;
        private const float MaximumReverseSpeed = 8f;
        private const float Acceleration = 14f;
        private const float BrakePower = 22f;
        private const float TurnRate = 125f;

        private Rigidbody body;
        private RaceManager race;
        private float throttle;
        private float steering;
        private float speed;
        private Vector3 startPosition;
        private Quaternion startRotation;

        public float Speed { get { return speed; } }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezePositionY |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationZ;

            var hitbox = GetComponent<BoxCollider>();
            hitbox.size = new Vector3(1.65f, 1.05f, 2.85f);
            hitbox.center = new Vector3(0f, 0.52f, 0f);
        }

        public void Initialize(RaceManager manager, Vector3 gridPosition, Quaternion gridRotation)
        {
            race = manager;
            startPosition = gridPosition;
            startRotation = gridRotation;
            ResetVehicle();
        }

        private void Update()
        {
            if (race == null) return;
            if (Input.GetKeyDown(KeyCode.Escape)) race.TogglePause();
            if (Input.GetKeyDown(KeyCode.R)) race.RestartTrial();

            if (!race.CanDrive)
            {
                throttle = 0f;
                steering = 0f;
                return;
            }

            throttle = (IsPressed(KeyCode.W, KeyCode.UpArrow) ? 1f : 0f) -
                       (IsPressed(KeyCode.S, KeyCode.DownArrow) ? 1f : 0f);
            steering = (IsPressed(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f) -
                       (IsPressed(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f);

        }

        private void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            if (race == null || !race.CanDrive)
            {
                throttle = 0f;
                steering = 0f;
                speed = Mathf.MoveTowards(speed, 0f, BrakePower * dt);
                return;
            }

            var target = throttle >= 0f ? throttle * MaximumSpeed : throttle * MaximumReverseSpeed;
            var acceleration = Mathf.Abs(throttle) < 0.01f ? BrakePower : Acceleration;
            speed = Mathf.MoveTowards(speed, target, acceleration * dt);

            var offRoad = TrackBuilder.DistanceFromRoad(transform.position) > TrackBuilder.TrackHalfWidth;
            var roadGrip = offRoad ? 0.42f : 1f;
            var speedFactor = Mathf.Lerp(0.24f, 1f, Mathf.Clamp01(Mathf.Abs(speed) / MaximumSpeed));
            var turn = steering * TurnRate * speedFactor * roadGrip * Mathf.Sign(speed) * dt;
            var nextRotation = body.rotation * Quaternion.Euler(0f, turn, 0f);
            var nextPosition = body.position + nextRotation * Vector3.forward * speed * roadGrip * dt;
            body.MoveRotation(nextRotation);
            body.MovePosition(new Vector3(nextPosition.x, startPosition.y, nextPosition.z));
        }

        public void ResetVehicle()
        {
            if (body == null) body = GetComponent<Rigidbody>();
            speed = 0f;
            throttle = 0f;
            steering = 0f;
            body.position = startPosition;
            body.rotation = startRotation;
            transform.SetPositionAndRotation(startPosition, startRotation);
        }

        private static bool IsPressed(KeyCode first, KeyCode second)
        {
            return Input.GetKey(first) || Input.GetKey(second);
        }
    }
}

using System;
using Artemis;
using Artemis.System;
using Microsoft.Xna.Framework;
using Twengine.Components;

namespace Twengine.SubSystems
{
    public class PhysicsSystem : EntityComponentProcessingSystem<Physics, Transform>
    {
        private float mWorldFriction;

        public PhysicsSystem() : base()
        {
            mWorldFriction = 0.8f;
        }

        public override void Process(Entity e, Physics physics, Transform transform)
        {
        
            Vector2 pos = transform.Position;

            if (physics.VelocityIsIntendedDirection)
            {
                TranslateMovementToVelocity(physics);
                LinearVelocityIntegration(transform, physics);
            }
            else
            {
                //TranslateForceToAcceleration(physics);
                //EulerIntegration(transform, physics);
                //VerletIntegration(transform, physics);
                LinearVelocityIntegration(transform, physics);
            }

            transform.OldPosition = pos;
        }

        private void TranslateForceToAcceleration(Physics physics)
        {
            //physics.Force = physics.IntendedMovementDirection * physics.MaxSpeed;
            //physics.Acceleration = physics.Force / physics.Mass;
        }

        private void TranslateMovementToVelocity(Physics physics)
        {
            physics.Velocity = physics.IntendedMovementDirection * physics.MaxSpeed;
        }

        private void LinearVelocityIntegration(Transform transform, Physics physics)
        {
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            transform.Position += physics.Velocity * deltaTimeInSeconds;
        }

        private void EulerIntegration(Transform transform, Physics physics)
        {
            // Newton's laws of motion, simple euler integration, 1 step
            Console.WriteLine("Doing Euler, Velocity: " + physics.Velocity);
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            physics.Velocity += physics.Acceleration * deltaTimeInSeconds;

            transform.Position += physics.Velocity * deltaTimeInSeconds;
        }

        /// <summary>
        /// Does not need velocity
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="physics"></param>
        private void VerletIntegration(Transform transform, Physics physics)
        {
            // Verlet integration from here: http://www.gamedev.net/page/resources/_/technical/math-and-physics/a-verlet-based-approach-for-2d-game-physics-r2714
            float deltaTimeInSeconds = entityWorld.Delta / 10000000.0f;
            transform.Position = ((2 - mWorldFriction)*transform.Position - (1 - mWorldFriction)*transform.OldPosition) +
                                  physics.Acceleration*(deltaTimeInSeconds);
        }
    }
}

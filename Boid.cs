using Aiv.Fast2D;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boids
{
    struct CollisionInfo
    {
        public Boid Collider;
        public Vector2 Delta;
    }

    class Boid
    {
        private static Texture texture;
        private Sprite sprite;
        public virtual Vector2 Position { get { return sprite.position; } set { sprite.position = value; } }
        public virtual float X { get { return sprite.position.X; } set { sprite.position.X = value; } }
        public virtual float Y { get { return sprite.position.Y; } set { sprite.position.Y = value; } }
        public Vector2 Forward { get => new Vector2((float)Math.Cos(sprite.Rotation), (float)Math.Sin(sprite.Rotation)); set => sprite.Rotation = (float)Math.Atan2(value.Y, value.X); }

        public float Width { get { return sprite.Width; } }
        public float Height { get { return sprite.Height; } }
        public float HalfWidth { get { return Width * 0.5f; } }
        public float HalfHeight { get { return Height * 0.5f; } }

        private Vector2 currentMoveSpeed;
        private Vector2 moveSpeed;
        private float forwardSpeed;
        private float shakeSpeed;
        private float separationSpeed;

        private Vector2 cohesionTarget;
        private bool cohesionTargetReached;
        private Vector2 forwardShakedTarget;
        private bool forwardShakedTargetReached;

        private float nearestBoidsRadius;
        private float nearestBoidsRadiusCohesion;

        private Timer timeToNextShake;
        private Timer timeToNextCohesion;

        private Sprite radius;

        public Boid(float spriteWidth = 0, float spriteHeight = 0)
        {
            spriteWidth = spriteWidth <= 0 ? Program.PixelsToUnits(texture.Width) : Program.PixelsToUnits(spriteWidth);
            spriteHeight = spriteHeight <= 0 ? Program.PixelsToUnits(texture.Height) : Program.PixelsToUnits(spriteHeight);

            sprite = new Sprite(spriteWidth, spriteHeight);
            sprite.pivot = new Vector2(spriteWidth * 0.5f, spriteHeight * 0.5f);

            Position = Program.Window.MousePosition;

            moveSpeed = new Vector2(3);
            currentMoveSpeed = moveSpeed;
            forwardSpeed = 2;
            shakeSpeed = 3;
            separationSpeed = 1;

            timeToNextShake = new Timer(1);
            timeToNextCohesion = new Timer(3, 3);

            Vector2 randDir;

            do
            {
                randDir = new Vector2(GetRandDirWithZero(), GetRandDirWithZero());
            }
            while (randDir == Vector2.Zero);

            if (randDir.Length > 1)
            {
                randDir.Normalize();
            }

            currentMoveSpeed *= randDir;

            nearestBoidsRadius = Math.Max(HalfWidth, HalfHeight) + 1;
            nearestBoidsRadiusCohesion = 3;

            radius = new Sprite(nearestBoidsRadius, nearestBoidsRadius);

            cohesionTargetReached = true;
            forwardShakedTargetReached = true;
        }

        private float GetRandDirWithZero()
        {
            int dir = RandomGenerator.GetRandomInt(0, 2);

            return RandomGenerator.GetRandomInt(0, 2) == 0 ? dir * -1 : dir * 1;
        }

        private float GetRandDir()
        {
            return RandomGenerator.GetRandomInt(0, 2) == 0 ? -1 : 1;
        }

        public static void LoadTexture()
        {
            texture = new Texture("Assets/boid.png");
        }

        public void Update()
        {
            Position += currentMoveSpeed * Program.DeltaTime;

            SetForwardAlignment();
            SetCohesion();
            SetDirAlignment();
            SetSeparation();

            CheckOutOfScreen();
        }

        private void SetForwardAlignment()
        {
            if (forwardShakedTargetReached)
            {
                timeToNextShake.Scale();

                Forward = Vector2.Lerp(Forward, currentMoveSpeed, forwardSpeed * Program.DeltaTime);
            }

            if (!forwardShakedTargetReached)
            {
                Forward = Vector2.Lerp(Forward, currentMoveSpeed + forwardShakedTarget, shakeSpeed * Program.DeltaTime);

                float forwardTargetAtan = (float)Math.Atan2(currentMoveSpeed.Y + forwardShakedTarget.Y, currentMoveSpeed.X + forwardShakedTarget.X);
                Vector2 forwardTarget = new Vector2((float)Math.Cos(forwardTargetAtan), (float)Math.Sin(forwardTargetAtan));

                Vector2 dist = forwardTarget - Forward;

                if (dist.Length <= 0.1f)
                {
                    forwardShakedTargetReached = true;
                }
            }

            else if (timeToNextShake.Clock <= 0)
            {
                Vector2 shakeForward = new Vector2(1 + RandomGenerator.GetRandomFloat(), 1 + RandomGenerator.GetRandomFloat());
                shakeForward.X *= GetRandDir();
                shakeForward.Y *= currentMoveSpeed.Y == 0 ? GetRandDir() : 0;

                forwardShakedTarget = shakeForward;
                forwardShakedTargetReached = false;

                timeToNextShake.Clock = RandomGenerator.GetRandomFloat();
            }
        }

        private void SetDirAlignment()
        {
            List<Boid> nearestBoids = GetNearestBoids(nearestBoidsRadius);

            if (nearestBoids.Count == 0)
            {
                return;
            }

            currentMoveSpeed = nearestBoids[0].currentMoveSpeed;
        }

        private List<Boid> GetNearestBoids(float radius)
        {
            List<Boid> nearestBoids = new List<Boid>();
            Vector2 distToOtherBoid;

            for (int i = 0; i < Program.Boids.Count; i++)
            {
                if (Program.Boids[i] == this)
                {
                    continue;
                }

                distToOtherBoid = Program.Boids[i].Position - Position;

                if (distToOtherBoid.LengthSquared <= radius * radius)
                {
                    nearestBoids.Add(Program.Boids[i]);
                }
            }

            return nearestBoids;
        }

        private void SetSeparation()
        {
            CollisionInfo collisionInfo = new CollisionInfo();

            if (CollidesWithNeighbour(ref collisionInfo))
            {
                if (collisionInfo.Delta.X < collisionInfo.Delta.Y)
                {
                    // Horizontal Collision
                    if (X < collisionInfo.Collider.X)
                    {
                        // Collision from Left (inverse horizontal delta)
                        collisionInfo.Delta.X = -collisionInfo.Delta.X - 1;
                    }

                    else
                    {
                        collisionInfo.Delta.X += 1;
                    }
                }

                else
                {
                    // Vertical Collision
                    if (Y < collisionInfo.Collider.Y)
                    {
                        // Collision from Left (inverse horizontal delta)
                        collisionInfo.Delta.Y = -collisionInfo.Delta.Y - 1;
                    }

                    else
                    {
                        collisionInfo.Delta.Y += 1;
                    }
                }

                Position = Vector2.Lerp(Position, Position + collisionInfo.Delta, separationSpeed * Program.DeltaTime);
            }
        }

        private void SetCohesion()
        {
            List<Boid> nearestBoids = GetNearestBoids(nearestBoidsRadiusCohesion);
            Vector2 positionAverage = Vector2.Zero;

            if (nearestBoids.Count <= 1)
            {
                return;
            }

            if (cohesionTargetReached)
            {
                timeToNextCohesion.Scale();
            }

            if (!cohesionTargetReached)
            {
                //we put a low blend speed 'cause Position is already moved by moveSpeed
                //in the Update
                Position = Vector2.Lerp(Position, Position + cohesionTarget, Program.DeltaTime * 0.1f);

                Vector2 dist = (Position + cohesionTarget) - Position;

                if (dist.Length <= 0.1f)
                {
                    cohesionTargetReached = true;
                }
            }

            else if (timeToNextCohesion.Clock <= 0)
            {
                for (int i = 0; i < nearestBoids.Count; i++)
                {
                    positionAverage += nearestBoids[i].Position;
                }

                positionAverage /= nearestBoids.Count;

                cohesionTarget = positionAverage - Position;

                cohesionTargetReached = false;

                timeToNextCohesion.Clock = 2 + RandomGenerator.GetRandomFloat();
            }
        }

        private bool CollidesWithNeighbour(ref CollisionInfo collisionInfo)
        {
            List<Boid> nearestBoids = GetNearestBoids(nearestBoidsRadius);

            for (int i = 0; i < nearestBoids.Count; i++)
            {
                Vector2 dist = nearestBoids[i].Position - Position;
                float deltaX = Math.Abs(dist.X) - (nearestBoids[i].HalfWidth + HalfWidth);
                float deltaY = Math.Abs(dist.Y) - (nearestBoids[i].HalfHeight + HalfHeight);

                if (deltaX <= 0 || deltaY <= 0)
                {
                    collisionInfo.Collider = nearestBoids[i];
                    collisionInfo.Delta = new Vector2(-deltaX, -deltaY);

                    return true;
                }
            }

            return false;
        }

        private void CheckOutOfScreen()
        {
            //horizontal collisions
            if (Position.X + HalfWidth < 0)
            {
                X += Program.OrthoWidth + 0.1f;
            }

            else if (Position.X - HalfWidth > Program.OrthoWidth)
            {
                X -= Program.OrthoWidth;
            }

            //vertical collisions
            if (Position.Y + HalfHeight < 0)
            {
                Y += Program.OrthoHeight + 0.1f;
            }

            else if (Position.Y - HalfHeight > Program.OrthoHeight)
            {
                Y -= Program.OrthoHeight;
            }
        }

        public void Draw()
        {
            sprite.DrawTexture(texture);

            //radius.position = sprite.position;
            //radius.DrawWireframe(1.0f, 0, 0);
        }
    }
}

using Aiv.Fast2D;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boids
{
    class Program
    {
        public static Window Window { get; private set; }
        public static float DeltaTime { get { return Window.DeltaTime; } }
        public static int WindowWidth { get { return Window.Width; } }
        public static int WindowHeight { get { return Window.Height; } }
        public static int HalfWindowWidth { get { return (int)(Window.Width * 0.5f); } }
        public static int HalfWindowHeight { get { return (int)(Window.Height * 0.5f); } }
        public static float OrthoWidth { get { return Window.OrthoWidth; } }
        public static float OrthoHeight { get { return Window.OrthoHeight; } }
        public static float OrthoHalfWidth { get { return Window.OrthoWidth * 0.5f; } }
        public static float OrthoHalfHeight { get { return Window.OrthoHeight * 0.5f; } }
        private static float optimalUnitSize;
        private static float optimalScreenHeight;
        private static bool isMousePressed;
        private static Vector2 lastMousePositionClicked;
        public static List<Boid> Boids;
        private static Timer timeToNextBoidSpawn;
        private static int numMaxBoids;

        static void Main(string[] args)
        {
            Init();
            Run();
        }

        static void Init()
        {
            Window = new Window(1280, 720, "Boids");
            Window.Position = Vector2.Zero;
            Window.SetDefaultViewportOrthographicSize(10);
            optimalScreenHeight = 1080;
            optimalUnitSize = optimalScreenHeight / Window.OrthoHeight;

            Boids = new List<Boid>();

            Boid.LoadTexture();

            timeToNextBoidSpawn = new Timer(0.2f);
            numMaxBoids = 100;
        }

        static void Run()
        {
            while (Window.IsOpened)
            {
                if (Window.GetKey(KeyCode.Esc))
                {
                    return;
                }

                if (Window.MouseLeft)
                {
                    if (!isMousePressed)
                    {
                        lastMousePositionClicked = Window.MousePosition;
                        isMousePressed = true;
                    }
                }

                else if (isMousePressed)
                {
                    timeToNextBoidSpawn.Clock = 0;

                    isMousePressed = false;
                }

                if (Window.MousePosition == lastMousePositionClicked &&
                    Window.MouseX >= -0.1178782f &&
                    Window.MouseX <= 17.84246f &&
                    Window.MouseY >= -0.476787925f &&
                    Window.MouseY <= -0.01254705f &&
                    isMousePressed &&
                    !Window.IsFullScreen())
                {
                    Window.Update();
                    continue;
                }

                Input();

                for (int i = 0; i < Boids.Count; i++)
                {
                    Boids[i].Update();

                    Boids[i].Draw();
                }

                //window update
                Window.Update();
            }
        }

        static void Input()
        {
            timeToNextBoidSpawn.Scale();

            if (Window.GetKey(KeyCode.Q) && timeToNextBoidSpawn.Clock <= 0 && Boids.Count <= numMaxBoids &&
                Window.MousePosition.X >= 0 && Window.MousePosition.X < OrthoWidth &&
                Window.MousePosition.Y >= 0 && Window.MousePosition.Y < OrthoHeight)
            {
                Boids.Add(new Boid());

                timeToNextBoidSpawn.Reset();
            }

            if (Window.MouseRight)
            {
                Boids.Clear();
                Console.Clear();
            }
        }

        public static float PixelsToUnits(float val)
        {
            return val / optimalUnitSize;
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace pg2Project
{
    public class Game : GameWindow
    {
        private Camera _camera;
        private readonly Stopwatch _gameTime = new();

        private Shader _shader;
        private Shader _lightshader;

        private bool _firstMove = true;
        private Vector2 _lastPos;

        private int _frameCount;

        //MESHES
        private Mesh _groundMesh;
        private Mesh _bunnyMesh;
        private Mesh _teaPot;


        //TEXTURES
        private Texture _groundTexture;
        private Texture _bunnyFur;
        private Texture _metal;

        private readonly Vector3 _lightPos = new Vector3(0f, 20f, 0f);

        public Game() : base(new GameWindowSettings(), new NativeWindowSettings())
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            _gameTime.Start();

            // Initialize OpenGL here
            GL.Enable(EnableCap.DebugOutput);

            //SHADERS
            _shader = new Shader("../../../assets/shader.vert", "../../../assets/shader.frag");
            _lightshader = new Shader("../../../assets/shader.vert", "../../../assets/light.frag");

            //TEXTURES
            _groundTexture = Texture.LoadFromFile("../../../textures/grass.png");
            _bunnyFur = Texture.LoadFromFile("../../../textures/giraffe.jpg");
            _metal = Texture.LoadFromFile("../../../textures/metal.png");

            //MESHES + POSITION SETUP
            _groundMesh = Mesh.Load("../../../objects/plane_tri_vnt.obj");

            _bunnyMesh = Mesh.Load("../../../objects/bunny_tri_vnt.obj");
            _bunnyMesh.Position = new Vector3(0, 0.25f, 0);

            _teaPot = Mesh.Load("../../../objects/teapot_tri_vnt.obj");
            _teaPot.Position = new Vector3(0, 0, 5);

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y)
            {
                Position = new Vector3(0, 1, 1)
            };
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            // Clear OpenGL canvas, both color buffer and Z-buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // Set color to Cyan
            GL.ClearColor(Color4.Black);

            _shader.Use();

            var model = Matrix4.Identity;
            _shader.SetMatrix4("model", model);
            _shader.SetMatrix4("view", _camera.GetViewMatrix());
            _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            _lightshader.SetVector3("lightColor", new Vector3(0.9f, 0.9f, 0.8f));

            _lightshader.SetVector3("lightPos", _lightPos);
            _lightshader.SetVector3("viewPos", (0, 50, 0));

            _lightshader.Use();

            _lightshader.SetMatrix4("model", Matrix4.Identity);
            _lightshader.SetMatrix4("view", _camera.GetViewMatrix());
            _lightshader.SetMatrix4("projection", _camera.GetProjectionMatrix());

            _groundTexture.Use(TextureUnit.Texture0);
            _groundMesh.Draw();
            _groundTexture.Unbind();

            _shader.SetMatrix4("model", Matrix4.CreateScale(0.08f) * Matrix4.CreateRotationY(_bunnyMesh.Rotation) * Matrix4.CreateTranslation(_bunnyMesh.Position));
            _bunnyFur.Use(TextureUnit.Texture0);
            _bunnyMesh.Draw();
            _bunnyFur.Unbind();

            _shader.SetMatrix4("model", Matrix4.CreateScale(0.03f) * Matrix4.CreateRotationY(_teaPot.Rotation) * Matrix4.CreateTranslation(_teaPot.Position));
            _metal.Use(TextureUnit.Texture0);
            _teaPot.Draw();
            _metal.Unbind();

            SwapBuffers();

            _frameCount++;
            double elapsed = _gameTime.Elapsed.TotalSeconds;
            if (elapsed >= 1.0)
            {
                int fps = (int)(_frameCount / elapsed);
                Console.WriteLine("FPS: " + fps);
                _frameCount = 0;
                _gameTime.Restart();
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }
            //Směrový vektor od teapotu k zajíci
            var direction = Vector3.Normalize(_bunnyMesh.Position - _teaPot.Position);
            //Vzdálenost mezi teapotem a zajícem
            var distance = Vector3.Distance(_bunnyMesh.Position, _teaPot.Position);
            //Pokud je vzdálenost menší než 0.3 pozice se resetují

            var angle = Math.Atan2(_bunnyMesh.Position[0] - _teaPot.Position[0], _bunnyMesh.Position[2] - _teaPot.Position[2]) - Math.PI / 2;

            if (distance < 0.3) 
            {
                _bunnyMesh.Position = new Vector3(0, 0.3f, 0);
                _teaPot.Position = new Vector3(0, 0, 5);

            }
            //Pohyb teapotu směrem k zajíci
            _teaPot.Position = _teaPot.Position + direction * 1 * (float)e.Time;
            _teaPot.Rotation = (float)angle;

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;
            
            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            if (input.IsKeyDown(Keys.Up))
            {   
                if(_bunnyMesh.Position[0] < 5)
                {
                    _bunnyMesh.Position += new Vector3(1, 0, 0) * 1.5f * (float)e.Time;
                }
                _bunnyMesh.Rotation = 3.14f;
            }
            if (input.IsKeyDown(Keys.Down))
            {
                if (_bunnyMesh.Position[0] > -5)
                {
                    _bunnyMesh.Position += new Vector3(-1, 0, 0) * 1.5f * (float)e.Time;
                }
                _bunnyMesh.Rotation = 0;

            }
            if (input.IsKeyDown(Keys.Left))
            {
                if (_bunnyMesh.Position[2] > -5)
                {
                    _bunnyMesh.Position += new Vector3(0, 0, -1) * 1.5f * (float)e.Time;
                }
                _bunnyMesh.Rotation = 4.7F;
            }
            if (input.IsKeyDown(Keys.Right))
            {
                if (_bunnyMesh.Position[2] < 5)
                {
                    _bunnyMesh.Position += new Vector3(0, 0, 1) * 1.5f * (float)e.Time;
                }
                _bunnyMesh.Rotation = 1.57F;
            }

            // Get the mouse state
            var mouse = MouseState;

            if (_firstMove) // This bool variable is initially set to true.
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {

                case Keys.V:
                    if (VSync == VSyncMode.On)
                    {
                        VSync = VSyncMode.Off;
                        Console.WriteLine("VSync: OFF");
                        break;
                    }
                    if (VSync == VSyncMode.Off)
                    {
                        VSync = VSyncMode.On;
                        Console.WriteLine("VSync: ON");
                        break;
                    }
                    break;

                case Keys.Escape:
                    _shader.Clear();
                    this.Close();
                    break;
                case Keys.F:
                    if(WindowState == WindowState.Normal)
                    {
                        WindowState = WindowState.Fullscreen;
                        break;
                    }
                    if (WindowState == WindowState.Fullscreen)
                    {
                        WindowState = WindowState.Normal;
                        break;
                    }
                    break;
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
            _camera.AspectRatio = Size.X / (float)Size.Y;

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left)
            {
                Console.WriteLine("Left mouse button pressed");
                return;
            }

            if (e.Button == MouseButton.Right)
            {
                Console.WriteLine("Right mouse button pressed");
                return;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            Console.WriteLine($"Mouse position: ({e.X}, {e.Y})");
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera.Fov -= e.OffsetY;

            Console.WriteLine($"Mouse wheel delta: {e.Offset}");
        }
    }
}

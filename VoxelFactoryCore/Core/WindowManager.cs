using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace VoxelFactoryCore
{
    public class WindowManager : GameWindow
    {
        public static WindowManager Inst;
        public WindowManager(double renderHz = 60, double updateHz = 50) : base(
            new GameWindowSettings()
            {
                RenderFrequency = renderHz,
                UpdateFrequency = updateHz
            }, new NativeWindowSettings()
            {
                Title = "Voxel Factory",
                APIVersion = new Version(4, 3),
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Size = new Vector2i(1280, 720)
            })
        {
            Inst = this;
        }

        private static void ReceiveMessage(DebugSource debugSource, DebugType type, int id, DebugSeverity severity, int len,
            IntPtr msgPtr, IntPtr customObj)
        {
            var msg = Marshal.PtrToStringAnsi(msgPtr, len);
            Console.WriteLine("Source {0}; Type {1}; id {2}; Severity {3}; msg: '{4}'", debugSource, type, id, severity, msg);
        }

        private static readonly DebugProcArb debugDelegate = new DebugProcArb(ReceiveMessage);

        private void TurnOnDebugging()
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GCHandle.Alloc(debugDelegate);
            var nullptr = new IntPtr(0);
            GL.Arb.DebugMessageCallback(debugDelegate, nullptr);
        }

        private int vao;
        private int quadProgram;

        private int cameraToWorldUniformQuad;
        private int cameraToWorldInverseUniformQuad;

        private int viewMatrixUniformQuad;
        private int projectionMatrixUniformQuad;

        private int modelMatrixQuad;

        private Camera camera;

        protected override void OnLoad()
        {
            TurnOnDebugging();
            Console.WriteLine($"Using OpenGL {GL.GetInteger(GetPName.MajorVersion)}.{GL.GetInteger(GetPName.MinorVersion)}");
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);

            vao = GenerateFullscreenQuadVAO();
            quadProgram = CreateQuadProgram();
            InitQuadProgram();

            camera = new Camera() { FOV = 75 };
            camera.Position = new Vector3d(0, 0, 0);

            camera.RecreateProjectionMatrix();

            InputManager.Initialize();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            InputManager.UpdateInput(this);

            camera.Update((float)args.Time);
            TraceTest();

            SwapBuffers();
        }

        private int CreateQuadProgram()
        {
            int quadProgram = GL.CreateProgram();
            int vshader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vshader, File.ReadAllText("quad.vertex"));
            GL.CompileShader(vshader);

            int fshader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fshader, File.ReadAllText("quad.fragment"));
            GL.CompileShader(fshader);

            GL.AttachShader(quadProgram, vshader);
            GL.AttachShader(quadProgram, fshader);

            GL.BindAttribLocation(quadProgram, 0, "vertex");
            GL.BindFragDataLocation(quadProgram, 0, "color");
            GL.LinkProgram(quadProgram);

            string programLog = GL.GetProgramInfoLog(quadProgram);
            if (string.IsNullOrEmpty(programLog) == false)
            {
                Console.WriteLine("Quad Program Log: " + programLog);
            }

            cameraToWorldUniformQuad = GL.GetUniformLocation(quadProgram, "_CameraToWorld");
            cameraToWorldInverseUniformQuad = GL.GetUniformLocation(quadProgram, "_CameraInverseProjection");
            return quadProgram;
        }

        private int GenerateFullscreenQuadVAO()
        {
            int vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);

            sbyte[] quadData = new sbyte[]
            {
                -1, -1,
                1, -1,
                1, 1,
                1, 1,
                -1, 1,
                -1, -1
            };
            GL.BufferData(BufferTarget.ArrayBuffer, quadData.Length, quadData, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Byte, false, 0, new IntPtr(0));
            GL.BindVertexArray(0);
            return vao;
        }

        private void InitQuadProgram()
        {
            GL.UseProgram(quadProgram);
            cameraToWorldUniformQuad = GL.GetUniformLocation(quadProgram, "_CameraToWorld");
            cameraToWorldInverseUniformQuad = GL.GetUniformLocation(quadProgram, "_CameraInverseProjection");
            viewMatrixUniformQuad = GL.GetUniformLocation(quadProgram, "_ViewMatrix");
            projectionMatrixUniformQuad = GL.GetUniformLocation(quadProgram, "_ProjMatrix");
            modelMatrixQuad = GL.GetUniformLocation(quadProgram, "_TransMatrix");
            GL.UseProgram(0);
        }

        private void TraceTest()
        {
            Matrix4 viewMatrix = Mathmatics.CreateViewMatrix(camera);
            Matrix4 projectionMatrix = camera.ProjectionMatrix;
            Matrix4 cameraWorldMatrix = viewMatrix * projectionMatrix;
            Matrix4 cameraWorldInverseMatrix = cameraWorldMatrix.Inverted();

            Matrix4 modelMatrix = Mathmatics.CreateTransformationMatrix(new Vector3d(0, 0, -1), Vector3d.Zero, Vector3d.One);

            GL.UseProgram(quadProgram);

            // Load up the compute shader with data
            GL.UniformMatrix4(cameraToWorldUniformQuad, false, ref cameraWorldMatrix);
            GL.UniformMatrix4(cameraToWorldInverseUniformQuad, false, ref cameraWorldInverseMatrix);
            GL.UniformMatrix4(modelMatrixQuad, false, ref modelMatrix);

            GL.UniformMatrix4(viewMatrixUniformQuad, false, ref viewMatrix);
            GL.UniformMatrix4(projectionMatrixUniformQuad, false, ref projectionMatrix);

            GL.BindVertexArray(vao);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }
    }
}

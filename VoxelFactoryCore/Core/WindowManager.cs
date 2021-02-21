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

        //private int computeProgramRenderTexture;
        private int vao;
        //private int computeProgram;
        private int quadProgram;

        //private int cameraToWorldUniform;
        //private int cameraToWorldInverseUniform;
        private int cameraToWorldUniformQuad;
        private int cameraToWorldInverseUniformQuad;
        //private int renderSizeUniform;

        //private int workGroupSizeX;
        //private int workGroupSizeY;

        private Camera camera;

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

        protected override void OnLoad()
        {
            TurnOnDebugging();
            Console.WriteLine($"Using OpenGL {GL.GetInteger(GetPName.MajorVersion)}.{GL.GetInteger(GetPName.MinorVersion)}");
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1f);

            //computeProgramRenderTexture = CreateFrameBuffer();
            vao = GenerateFullscreenQuadVAO();
            //computeProgram = GenerateComputeProgram("raytracer.compute");
            //InitComputeProgram();
            quadProgram = CreateQuadProgram();
            InitQuadProgram();

            camera = new Camera() { FOV = 60 };
            camera.Position = new Vector3d(3, 2, 7);

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

        /*
        public static int GenerateComputeProgram(string computeShaderPath)
        {
            int shader = GL.CreateShader(ShaderType.ComputeShader);
            GL.ShaderSource(shader, File.ReadAllText(computeShaderPath));
            GL.CompileShader(shader);

            string vertexShaderLog = GL.GetShaderInfoLog(shader);
            if (string.IsNullOrEmpty(vertexShaderLog) == false)
            {
                Console.WriteLine("Compute shader log: " + vertexShaderLog);
            }

            int shaderProgram = GL.CreateProgram();

            GL.AttachShader(shaderProgram, shader);

            GL.LinkProgram(shaderProgram);
            GL.ValidateProgram(shaderProgram);

            GL.DetachShader(shaderProgram, shader);
            GL.DeleteShader(shader);

            return shaderProgram;
        }*/

        /*
        private int CreateFrameBuffer()
        {
            int frameTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, frameTex);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, new int[] { (int)TextureMagFilter.Nearest });
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, new int[] { (int)TextureMinFilter.Nearest });


            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Size.X, Size.Y, 0, PixelFormat.Rgba, PixelType.Float, new IntPtr(0));
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return frameTex;
        }*/

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
                Console.WriteLine(programLog);
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

        /*
        private void InitComputeProgram()
        {
            GL.UseProgram(computeProgram);

            GL.GetInteger((GetIndexedPName)GetProgramParameterName.MaxComputeWorkGroupSize, 0, out int workGroupSize);
            workGroupSizeX = workGroupSize;
            workGroupSizeY = workGroupSize;

            Console.WriteLine("Work group size: " + workGroupSize);
            
            cameraToWorldUniform = GL.GetUniformLocation(computeProgram, "_CameraToWorld");
            cameraToWorldInverseUniform = GL.GetUniformLocation(computeProgram, "_CameraInverseProjection");
            renderSizeUniform = GL.GetUniformLocation(computeProgram, "_RenderSize");
            GL.UseProgram(0);
        }*/

        private void InitQuadProgram()
        {
            GL.UseProgram(quadProgram);
            //int texUniform = GL.GetUniformLocation(quadProgram, "tex");
            //GL.Uniform1(texUniform, 0);
            cameraToWorldUniformQuad = GL.GetUniformLocation(quadProgram, "_CameraToWorld");
            cameraToWorldInverseUniformQuad = GL.GetUniformLocation(quadProgram, "_CameraInverseProjection");
            GL.UseProgram(0);
        }

        private void TraceTest()
        {
            //GL.UseProgram(computeProgram);

            Matrix4 viewMatrix = Mathmatics.CreateViewMatrix(camera);
            Matrix4 projectionMatrix = camera.ProjectionMatrix;
            Matrix4 cameraWorldMatrix = projectionMatrix * viewMatrix;
            Matrix4 cameraWorldInverseMatrix = cameraWorldMatrix.Inverted();

            /*
            // Load up the compute shader with data
            GL.UniformMatrix4(cameraToWorldUniform, true, ref cameraWorldMatrix);
            GL.UniformMatrix4(cameraToWorldInverseUniform, true, ref cameraWorldInverseMatrix);
            GL.Uniform2(renderSizeUniform, Size.X, Size.Y);

            // Bind the texture for the compute shader
            GL.BindImageTexture(0, computeProgramRenderTexture, 0, false, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba32f);

            // Should be used, but we aren't.
            int worksizeX = NextPowerOf2(Size.X);
            int worksizeY = NextPowerOf2(Size.Y);

            // Run the compute shader without trying to run it more than required
            //GL.DispatchCompute(worksizeX / workGroupSizeX / 8, worksizeY / workGroupSizeY / 8, 1);
            GL.DispatchCompute(Size.X / 8 + (Size.X % 8 != 0 ? 1 : 0), Size.Y / 8 + (Size.Y % 8 != 0 ? 1 : 0), 1);

            // Cleanup
            GL.BindImageTexture(0, 0, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
            GL.UseProgram(0);*/

            // Draw the computed texture to the screen, basically 1:1
            GL.UseProgram(quadProgram);

            // Load up the compute shader with data
            GL.UniformMatrix4(cameraToWorldUniformQuad, true, ref cameraWorldMatrix);
            GL.UniformMatrix4(cameraToWorldInverseUniformQuad, true, ref cameraWorldInverseMatrix);

            GL.BindVertexArray(vao);
            //GL.BindTexture(TextureTarget.Texture2D, computeProgramRenderTexture);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
        }

        /*
        private static int NextPowerOf2(int x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            x++;
            return x;
        }*/
    }
}

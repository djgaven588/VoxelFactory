using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace VoxelFactoryCore
{
    public static class RenderShortcut
    {
        public static int GenerateProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            int vertShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertShader, File.ReadAllText(vertexShaderPath));
            GL.CompileShader(vertShader);

            string vertexShaderLog = GL.GetShaderInfoLog(vertShader);
            if (string.IsNullOrEmpty(vertexShaderLog) == false)
            {
                Console.WriteLine("Vertex shader log: " + vertexShaderLog);
            }

            int fragShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragShader, File.ReadAllText(fragmentShaderPath));
            GL.CompileShader(fragShader);

            string fragmentShaderLog = GL.GetShaderInfoLog(fragShader);
            if (string.IsNullOrEmpty(vertexShaderLog) == false)
            {
                Console.WriteLine("Fragment shader log: " + fragmentShaderLog);
            }

            int shaderProgram = GL.CreateProgram();

            GL.AttachShader(shaderProgram, vertShader);
            GL.AttachShader(shaderProgram, fragShader);

            GL.LinkProgram(shaderProgram);
            GL.ValidateProgram(shaderProgram);

            GL.DetachShader(shaderProgram, vertShader);
            GL.DetachShader(shaderProgram, fragShader);
            GL.DeleteShader(vertShader);
            GL.DeleteShader(fragShader);

            return shaderProgram;
        }
    }
}

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace shdr.Engine
{
   // taken from https://github.com/opentk/LearnOpenTK/blob/master/Common/Shader.cs
   public class __shader
   {
      private static int _active;
      private readonly int _handle;
      private readonly Dictionary<string, int> _uniformLocations;

      public __shader(string vertPath, string fragPath, string geomPath = null)
      {
         string shaderSource = File.ReadAllText(vertPath);
         int vertexShader = GL.CreateShader(ShaderType.VertexShader);
         GL.ShaderSource(vertexShader, shaderSource);
         compile_shader(vertexShader);

         shaderSource = File.ReadAllText(fragPath);
         int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
         GL.ShaderSource(fragmentShader, shaderSource);
         compile_shader(fragmentShader);

         int geometryShader = -1;
         if (geomPath != null)
         {
            shaderSource = File.ReadAllText(geomPath);
            geometryShader = GL.CreateShader(ShaderType.GeometryShader);
            GL.ShaderSource(geometryShader, shaderSource);
            compile_shader(geometryShader);
         }

         _handle = GL.CreateProgram();

         GL.AttachShader(_handle, vertexShader);
         GL.AttachShader(_handle, fragmentShader);
         if (geometryShader != -1) GL.AttachShader(_handle, geometryShader);

         link_program(_handle);

         GL.DetachShader(_handle, vertexShader);
         GL.DetachShader(_handle, fragmentShader);
         if (geometryShader != -1) GL.DetachShader(_handle, geometryShader);
         GL.DeleteShader(fragmentShader);
         GL.DeleteShader(vertexShader);
         if (geometryShader != -1) GL.DeleteShader(geometryShader);

         GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

         _uniformLocations = new Dictionary<string, int>();

         for (int i = 0; i < numberOfUniforms; i++)
         {
            string key = GL.GetActiveUniform(_handle, i, out _, out _);
            int location = GL.GetUniformLocation(_handle, key);
            _uniformLocations.Add(key, location);
         }
      }

      private static void compile_shader(int shader)
      {
         GL.CompileShader(shader);

         GL.GetShader(shader, ShaderParameter.CompileStatus, out int code);
         if (code == (int)All.True) return;
         string infoLog = GL.GetShaderInfoLog(shader);
         throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
      }

      private static void link_program(int program)
      {
         GL.LinkProgram(program);

         GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int code);
         if (code == (int)All.True) return;
         string infoLog = GL.GetProgramInfoLog(program);
         throw new Exception($"Error occurred whilst linking Program({program}) \n\n{infoLog}");
      }

      public void bind()
      {
         if (_handle == _active) return;
         GL.UseProgram(_handle);
         _active = _handle;
      }

      public static void unbind()
      {
         GL.UseProgram(0);
         _active = 0;
      }

      public int get_attrib_location(string attribName)
      {
         return GL.GetAttribLocation(_handle, attribName);
      }

      /// <summary>
      ///    Set a uniform int on this shader.
      /// </summary>
      /// <param name="name">The name of the uniform</param>
      /// <param name="data">The data to set</param>
      public void set_int(string name, int data)
      {
         if (!_uniformLocations.ContainsKey(name)) return;
         bind();
         GL.Uniform1(_uniformLocations[name], data);
      }

      /// <summary>
      ///    Set a uniform float on this shader.
      /// </summary>
      /// <param name="name">The name of the uniform</param>
      /// <param name="data">The data to set</param>
      public void set_float(string name, float data)
      {
         if (!_uniformLocations.ContainsKey(name)) return;
         bind();
         GL.Uniform1(_uniformLocations[name], data);
      }

      /// <summary>
      ///    Set a uniform Matrix4 on this shader
      /// </summary>
      /// <param name="name">The name of the uniform</param>
      /// <param name="data">The data to set</param>
      /// <remarks>
      ///    <para>
      ///       The matrix is transposed before being sent to the shader.
      ///    </para>
      /// </remarks>
      public void set_matrix4(string name, Matrix4 data)
      {
         if (!_uniformLocations.ContainsKey(name)) return;
         bind();
         GL.UniformMatrix4(_uniformLocations[name], true, ref data);
      }

      /// <summary>
      ///    Set a uniform Vector3 on this shader.
      /// </summary>
      /// <param name="name">The name of the uniform</param>
      /// <param name="data">The data to set</param>
      public void set_vector3(string name, Vector3 data)
      {
         if (!_uniformLocations.ContainsKey(name)) return;
         bind();
         GL.Uniform3(_uniformLocations[name], data);
      }

      /// <summary>
      ///    Set a uniform Vector2 on this shader.
      /// </summary>
      /// <param name="name">The name of the uniform</param>
      /// <param name="data">The data to set</param>
      public void set_vector2(string name, Vector2 data)
      {
         if (!_uniformLocations.ContainsKey(name)) return;
         bind();
         GL.Uniform2(_uniformLocations[name], data);
      }
   }
}
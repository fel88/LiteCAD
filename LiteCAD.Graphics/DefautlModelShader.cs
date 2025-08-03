namespace LiteCAD.Graphics
{
    public class DefaultModelShader : Shader
    {
        public DefaultModelShader()
        {
            InitFromResources("cam_space_shader.fs", "cam_space_shader.vs");
        }

    }
}

using Engine.Input;
using Engine.Windowing;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Engine.Renderable
{
    public class ImGUI
    {
        ImGuiIOPtr io;
        ImGUI()
        {
            ImGui.CreateContext();
            io = ImGui.GetIO();
            ImGui.StyleColorsDark();
            ImGuiController controller = new ImGuiController(WindowClass.GlHandle, WindowClass.Handle, InputHandler._context);

        }
        
        void TestGUI()
        {
            ImGui.Begin("Testwindow");
            ImGui.NewFrame();
            ImGui.CaptureKeyboardFromApp(true);
            ImGui.CaptureMouseFromApp(true);
        }
    }
}
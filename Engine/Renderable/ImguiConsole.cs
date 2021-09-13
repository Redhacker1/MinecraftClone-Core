using Engine.Windowing;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;
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
            
        }
        
        void TestGUI()
        {
            ImGui.Begin("Testwindow");
            ImGui.NewFrame();
            ImGui.CaptureKeyboardFromApp(true);
            ImGui.CaptureMouseFromApp(true);
            //Im
        }
    }
}
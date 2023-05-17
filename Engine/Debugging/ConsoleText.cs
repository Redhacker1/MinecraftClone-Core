using System.Numerics;
using Engine.Renderable;
using ImGuiNET;

namespace Engine.Debugging
{
    // TODO: When I write my own UI solution, move this over to it!
    public class ConsoleText : ImGUIPanel
    {
        byte[] test = new byte[100];
        string Scrollback = string.Empty;
        string Test_Results = "";
        public ConsoleText()
        {
            ConsoleLibrary.InitConsole(SetConsoleScrollback);
            PanelName = "Console Window";
        }


        public void SetConsoleScrollback(string new_scrollback)
        {
            Scrollback = new_scrollback;
        }
        public override unsafe void CreateUI()
        {
            string scrollback = ConsoleLibrary.GetScrollback();
            
            ImGui.SetNextItemWidth(ImGui.GetWindowSize().X - (ImGui.CalcTextSize("Submit").X * 2));
            ImGui.InputTextMultiline("##Scrollback", ref scrollback, (uint) Scrollback.Length,
                new Vector2(ImGui.GetWindowSize().X - 20, ImGui.GetWindowSize().Y - 90), ImGuiInputTextFlags.ReadOnly);


            ImGui.Columns(2, "", true);
            var modified = ImGui.InputText("##CurrentCommand",ref Test_Results, ushort.MaxValue, ImGuiInputTextFlags.EnterReturnsTrue);
            ImGui.SetScrollX(ImGui.GetScrollMaxX());
            if (modified)
            {
                ConsoleLibrary.process_command(Test_Results);
                Test_Results = "";
                
                ImGui.SetKeyboardFocusHere(-1);
            }
            ImGui.SetColumnWidth(0, ImGui.GetWindowSize().X - (ImGui.CalcTextSize("Submit").X * 2));
            ImGui.NextColumn();
            bool selected = ImGui.Button("Submit");
            if (selected)
            {
                ConsoleLibrary.process_command(Test_Results);
                Test_Results = "";
                
                ImGui.SetKeyboardFocusHere(-1);
            }
        }
        
    }
}
using System;
using System.Numerics;
using Engine.Renderable;
using ImGuiNET;

namespace Engine.Debug
{
    public class ConsoleText : ImGUIPanel
    {
        byte[] test = new byte[100];
        string Scrollback = string.Empty;
        string Test_Results = "";
        public ConsoleText()
        {
            PanelName = "Console Window";
        }


        public void SetConsoleScrollback(string new_scrollback)
        {
            Scrollback = new_scrollback;
        }
        public override unsafe void CreateUI()
        {

            string scrollback = ConsoleLibrary.GetScrollback();
            ImGui.InputTextMultiline("##Scrollback", ref scrollback, (uint) Scrollback.Length,
                new Vector2(ImGui.GetWindowSize().X - 20, ImGui.GetWindowSize().Y - 90), ImGuiInputTextFlags.ReadOnly);

            ImGui.Columns(2, "", true);
            var modified = ImGui.InputText("##CurrentCommand",ref Test_Results, 4096, ImGuiInputTextFlags.EnterReturnsTrue, Callback);
            ImGui.SetScrollX(ImGui.GetScrollMaxX());
            //var modified = ImGui.InputText(string.Empty, ref Test_Results, 100, ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackEdit, Callback  );
            if (modified)
            {
                ConsoleLibrary.process_command(Test_Results);
                Test_Results = "";
                
                ImGui.SetKeyboardFocusHere(-1);
            }
            ImGui.NextColumn();
            ImGui.Button("Submit");

            //ImGui.SetScrollY(10);
        }
        

        unsafe int Callback(ImGuiInputTextCallbackData* data)
        {
            Console.WriteLine(data->EventChar);
            Console.WriteLine("Callback called!");
            return 1;
        }
    }
}
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Engine.Renderable;
using ImGuiNET;

namespace MCClone_Core.Debug_and_Logging
{
    public class ConsoleText : ImGUIPanel
    {

        string Scrollback = string.Empty;
        byte[] buffer = new byte[1000];
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
            ImGui.Text("Hello");
            ImGui.InputTextMultiline(string.Empty, ref Scrollback, (uint) Scrollback.Length,
                new Vector2(ImGui.GetWindowSize().X - 30, ImGui.GetWindowSize().Y - 90));

            var item = this;
            var modified = ImGui.InputText(string.Empty, buffer, 1000, ImGuiInputTextFlags.EnterReturnsTrue, null, (IntPtr)Unsafe.AsPointer(ref item) );

            if (modified)
            {
                ConsoleLibrary.process_command(Encoding.UTF8.GetString(buffer));
            }
            //ImGui.SetScrollY(10);
        }
    }
}
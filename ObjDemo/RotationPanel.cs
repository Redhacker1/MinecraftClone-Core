using System.Numerics;
using Engine.Objects;
using Engine.Renderable;
using ImGuiNET;

namespace ObjDemo
{
    public class RotationPanel : ImGUIPanel
    {
        GameObject ObjectReference;

        public RotationPanel(GameObject rotationObject)
        {
            ObjectReference = rotationObject;
        }

        public override void CreateUI()
        {
            Vector3 Rotation = ObjectReference.Rotation;

            if (ImGui.InputFloat3("Rotation", ref Rotation))
            {
                ObjectReference.Rotation = Rotation;
            }
            
            
        }
    }
}
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
            Quaternion Rotation = ObjectReference.Rotation;
            Vector4 Rotation4 = new Vector4(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W);

            if (ImGui.InputFloat4("Rotation", ref Rotation4))
            {
                ObjectReference.Rotation = new Quaternion(Rotation4.X, Rotation4.Y, Rotation4.Z, Rotation4.W);
            }
            
            
        }
    }
}
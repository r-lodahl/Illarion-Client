using Godot;
using Illarion.Client.EngineBinding.Interface;

namespace Illarion.Client.EngineBinding.Godot
{
    public class MathWrapper : IMath
    {
        public int Ceil(float value) => (int)Mathf.Ceil(value);
    }
}
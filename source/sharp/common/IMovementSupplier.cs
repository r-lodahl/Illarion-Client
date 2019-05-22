using System;
using Godot;

public interface IMovementSupplier
{
    event EventHandler<Vector2i> movementDone;
    event EventHandler<int> layerChanged;
}
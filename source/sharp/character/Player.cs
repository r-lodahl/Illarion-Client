using System;
using Godot;
using Illarion.Client.Common;

namespace Illarion.Client.Character
{
    public class Player : KinematicBody2D, IMovementSupplier
    {
        Vector2 currentPosition;

        public event EventHandler<Vector2i> MovementDone;
        public event EventHandler<int> LayerChanged;

        protected virtual void OnMovementDone(int x, int y)
        {
            EventHandler<Vector2i> handler = MovementDone;
            handler?.Invoke(this, new Vector2i(x,y));
        }

        protected virtual void OnLayerChanged(int layer)
        {
            EventHandler<int> handler = LayerChanged;
            handler?.Invoke(this, layer);
        }

        public override void _Ready()
        {
            currentPosition = Position;
            SetProcessInput(true);
            SetProcess(true);
        }

        public override void _Process(float delta) 
        {
            Position = currentPosition;
        }

        public override void _Input(InputEvent @event)
        {
            int x = 0;
            int y = 0;

            if (Input.IsActionPressed("move_up"))
            {
                currentPosition.y -= 18.5f;
                currentPosition.x -= 38f;
                x = -1;
            }
            else if (Input.IsActionPressed("move_down"))
            {
                currentPosition.y += 18.5f;
                currentPosition.x += 38f;
                x = 1;
            } 

            if (Input.IsActionPressed("move_left")) 
            {
                currentPosition.y += 18.5f;
                currentPosition.x -= 38f;
                y = -1;
            }
            else if (Input.IsActionPressed("move_right"))
            {
                currentPosition.y -= 18.5f;
                currentPosition.x += 38f;
                y = 1;
            }

            if (x == 0  && y == 0) return;

            OnMovementDone(x, y);
        }

    }
}
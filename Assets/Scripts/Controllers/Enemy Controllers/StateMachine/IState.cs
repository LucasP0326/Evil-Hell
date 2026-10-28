using UnityEngine;

public interface IState
{
    void Enter();   // Runs once when switching into this state
    void Update();  // Runs every frame while in this state
    void Exit();    // Runs once when switching out of this state
}

 # Advanced State Machine
 
 ### Example of states:
 #### IdleState.cs
 ```csharp
public class IdleState : StateMachine.IState
{
    private readonly Character character;

    public IdleState(Character character)
    {
        this.character = character;
    }

    public void OnEnter()
    {
        character.PlayAnimation("Idle");
    }

    public void OnUpdate(float deltaTime)
    {
        if (character.Velocity.magnitude > 0.1f)
        {
            character.StateMachine.Change<MoveState>();
        }
    }

    public void OnExit()
    {
        // Optional cleanup
    }
}

```

 #### MoveState.cs
 ```csharp
public class MoveState : StateMachine.IState
{
    private readonly Character character;

    public MoveState(Character character)
    {
        this.character = character;
    }

    public void OnEnter()
    {
        character.PlayAnimation("Run");
    }

    public void OnUpdate(float deltaTime)
    {
        if (character.Velocity.magnitude < 0.1f)
        {
            character.StateMachine.Change<IdleState>();
        }
    }

    public void OnExit()
    {
        // Optional cleanup
    }
}

```

### Multiplayer usage:
 ```csharp
//On the server, you could do something like:
if (serverShouldSwitchToAttack)
{
    character.StateMachine.ForceChange(typeof(AttackState));
    Rpc_ClientSyncState("AttackState");
}
//On client:
void Rpc_ClientSyncState(string stateName)
{
    var type = Type.GetType(stateName);
    character.StateMachine.ForceChange(type);
}
//NOTE: You may want to map safe enums or hash IDs instead of raw type names in production for security/networking.
```
 ### Example of usage
 ```
using UnityEngine;

public class Character : MonoBehaviour
{
    public Vector2 Velocity { get; private set; }
    public StateMachine StateMachine { get; private set; }

    private void Start()
    {
        StateMachine = new StateMachine();

        // Register all states
        StateMachine.Register<IdleState>(new IdleState(this));
        StateMachine.Register<MoveState>(new MoveState(this));

        StateMachine.Change<IdleState>();
    }

    private void Update()
    {
        // Simulate input or network velocity
        Velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        StateMachine.Update(Time.deltaTime);
    }

    public void PlayAnimation(string name)
    {
        Debug.Log($"[Character] Playing animation: {name}");
        // Hook into Animator if needed
    }
}

```

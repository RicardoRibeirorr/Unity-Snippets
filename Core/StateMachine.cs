 /**
 * State machine system
 * 
 * @author Ricardo Ribeiro - Fantasy2DRPG 
 * @url: https://github.com/RicardoRibeirorr/Unity-Snippets
 * 
 **/

using System;
using System.Collections.Generic;

public class StateMachine
{
    public interface IState
    {
        public void OnEnter();
        public void OnExit();
    }

    public event Action<StateMachine> OnChanged;

    public IState CurrentState { get; private set; }
    public IState PreviousState { get; private set; }

    readonly Dictionary<Type, IState> states = new Dictionary<Type, IState>();

    public StateMachine(IState defaultState)
    {
        CurrentState = defaultState;
    }

    public void Change<T>() where T : IState
    {
        if (!TryGet<T>(out var state))
        {
            throw new KeyNotFoundException();
        }

        PreviousState = CurrentState;
        CurrentState = state;

        PreviousState.OnExit();
        CurrentState.OnEnter();

        OnChanged?.Invoke(this);
    }

    public bool IsIn<T>() where T : IState
    {
        return CurrentState is T;
    }

    public void Register<T>() where T : IState, new()
    {
        var key = typeof(T);
        var state = new T();
        states.Add(key, state);
    }

    public bool TryGet<T>(out IState state) where T : IState
    {
        var key = typeof(T);
        return states.TryGetValue(key, out state);
    }
}

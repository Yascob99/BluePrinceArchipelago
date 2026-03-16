using BluePrinceArchipelago.Utils.Actions;
using HutongGames.PlayMaker;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections;
using System.Linq;
using System.Transactions;
using UnityEngine;

namespace BluePrinceArchipelago.Utils
{
    // Not going to lie alot of this is borrowed from: https://github.com/hk-modding/HK.Core.FsmUtil/ with barely any modification. Full Credit to the HK modding team.
    public static partial class FsmUtil
    {

        /// <summary>
        ///     Gets a PlayMakerArrayListProxy.
        /// </summary>
        /// <param name="go">The GameObject</param>
        /// <param name="arrayListProxyName">The name of the PlayMakerArrayListProxy</param>
        /// <returns>The found PlayMakerArrayListProxy, null if it isn't found</returns>
        public static PlayMakerArrayListProxy GetArrayListProxy(this GameObject go, string arrayListProxyName)
        {
            foreach (PlayMakerArrayListProxy arrayListProxy in go.GetComponents<PlayMakerArrayListProxy>())
            {
                if (arrayListProxy.name == arrayListProxyName)
                {
                    return arrayListProxy;
                }
            }
            return null;
        }

        /// <summary>
        ///     Gets the Count of a PlayMakerArrayListProxy.
        /// </summary>
        /// <param name="arrayListProxy">The PlayMakerArrayListProxy</param>
        /// <returns>Returns the Count of the PlayMakerArrayListProxy, 0 if it isn't found</returns>
        public static int GetCount(this PlayMakerArrayListProxy arrayListProxy)
        {
            Il2CppSystem.Collections.ArrayList arrayList = arrayListProxy.arrayList;
            if (arrayList != null)
            {
                return arrayList.Count;
            }
            return 0;
        }
        /// <summary>
        ///     Checks if the PlayMakerArrayListProxy contains a value
        /// </summary>
        /// <param name="arrayListProxy">The PlayMakerArrayListProxy</param>
        /// <param name="value">The Object to add to the Array</param>
        /// <returns>Returns if the given item is in an ArrayListProxy, false if the array can't be retrieved.</returns>
        public static bool Contains(this PlayMakerArrayListProxy arrayListProxy, Il2CppSystem.Object value)
        {
            Il2CppSystem.Collections.ArrayList arrayList = arrayListProxy.arrayList;
            if (arrayList != null)
            {
                return arrayList.Contains(value);
            }
            return false;
        }

        /// <summary>
        ///     Removes a value at an index from a PlayMakerArrayListProxy 
        /// </summary>
        /// <param name="arrayListProxy">The PlayMakerArrayListProxy</param>
        /// <param name="index">The index of the item to remove from the array.</param>
        public static void RemoveAt(this PlayMakerArrayListProxy arrayListProxy, int index)
        {
            Il2CppSystem.Collections.ArrayList arrayList = arrayListProxy.arrayList;
            if (arrayList != null && arrayList.Count > index && index > -1)
            {
                arrayListProxy.Remove(arrayList[index], arrayList[index].GetType().ToString()); //Use the in built remove function to make sure nothing in the PlayMakerArrayListProxy Breaks.
            }
        }

        /// <summary>
        ///     Gets a value at an index from a PlayMakerArrayListProxy
        /// </summary>
        /// <param name="arrayListProxy">The PlayMakerArrayListProxy</param>
        /// <param name="index">The index to retrieve</param>
        /// <returns>Returns an item from the index of the ArrayListProxy, returns null if the index is out of range.</returns>
        public static Il2CppSystem.Object GetItemAt(this PlayMakerArrayListProxy arrayListProxy, int index)
        {
            Il2CppSystem.Collections.ArrayList arrayList = arrayListProxy.arrayList;
            if (arrayList != null && arrayList.Count > index && index > -1)
            {
                return arrayList[index];
            }
            return null;
        }

        /// <summary>
        ///     Gets an FSM from a Unity GameObject
        /// </summary>
        /// <param name="go">The GameObject</param>
        /// <param name="fsmName">The name of the FSM to get</param>
        /// <returns>Returns the fsm with the given name, returns null if it can't be found.</returns>
        public static PlayMakerFSM GetFsm(this GameObject go, string fsmName)
        {
            foreach (PlayMakerFSM fsm in go.GetComponents<PlayMakerFSM>())
            {
                if (fsm.FsmName == fsmName)
                {
                    return fsm;
                }
            }
            return null;
        }
        /// <summary>
        ///     Gets a preprocessed FSM from a Unity GameObject
        /// </summary>
        /// <param name="go">The GameObject</param>
        /// <param name="fsmName">The name of the FSM to get</param>
        /// <returns>Returns the preprocessed fsm with the given name, returns null if it can't be found.</returns>
        public static PlayMakerFSM GetFsmPreprocessed(this GameObject go, string fsmName)
        {
            foreach (PlayMakerFSM fsm in go.GetComponents<PlayMakerFSM>())
            {
                if (fsm.FsmName == fsmName)
                {
                    fsm.Preprocess();
                    return fsm;
                }
            }
            return null;
        }
        private static TVal GetItemFromArray<TVal>(TVal[] origArray, Func<TVal, bool> isItemCheck) where TVal : class
        {
            foreach (TVal item in origArray)
            {
                if (isItemCheck(item))
                {
                    return item;
                }
            }
            return null;
        }

        private static TVal[] GetItemsFromArray<TVal>(TVal[] origArray, Func<TVal, bool> isItemCheck) where TVal : class
        {
            int foundItems = 0;
            foreach (TVal item in origArray)
            {
                if (isItemCheck(item))
                {
                    foundItems++;
                }
            }
            if (foundItems == origArray.Length)
            {
                return origArray;
            }
            if (foundItems == 0)
            {
                return [];
            }
            TVal[] retArray = new TVal[foundItems];
            int foundProgress = 0;
            foreach (TVal item in origArray)
            {
                if (isItemCheck(item))
                {
                    retArray[foundProgress] = item;
                    foundProgress++;
                }
            }
            return retArray;
        }
        public static FsmTransition GetTransition(this PlayMakerFSM fsm, string stateName, string eventName) => fsm.GetState(stateName)!.GetTransition(eventName);

        /// <inheritdoc cref="GetTransition(PlayMakerFSM, string, string)"/>
        public static FsmTransition GetTransition(this Fsm fsm, string stateName, string eventName) => fsm.GetState(stateName)!.GetTransition(eventName);

        /// <inheritdoc cref="GetTransition(PlayMakerFSM, string, string)"/>
        /// <param name="state">The state</param>
        /// <param name="eventName">The name of the event</param>
        public static FsmTransition GetTransition(this FsmState state, string eventName) => GetItemFromArray<FsmTransition>(state.Transitions, x => x.EventName == eventName);

        /// <summary>
        ///     Gets a global transition in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="globalEventName">The name of the event</param>
        /// <returns>The found global transition, null if none are found</returns>
        public static FsmTransition GetGlobalTransition(this PlayMakerFSM fsm, string globalEventName) => fsm.Fsm.GetGlobalTransition(globalEventName);

        /// <inheritdoc cref="GetGlobalTransition(PlayMakerFSM, string)"/>
        public static FsmTransition GetGlobalTransition(this Fsm fsm, string globalEventName) => GetItemFromArray<FsmTransition>(fsm.GlobalTransitions, x => x.ToState == globalEventName);

        /// <summary>
        ///     Gets an action in a PlayMakerFSM.
        /// </summary>
        /// <typeparam name="TAction">The type of the action that is wanted</typeparam>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state</param>
        /// <param name="index">The index of the action</param>
        /// <returns>The action, null if it can't be found</returns>

        public static TAction GetAction<TAction>(this PlayMakerFSM fsm, string stateName, int index) where TAction : FsmStateAction => fsm.GetState(stateName)!.GetAction<TAction>(index);

        /// <inheritdoc cref="GetAction{TAction}(PlayMakerFSM, string, int)"/>

        public static TAction GetAction<TAction>(this Fsm fsm, string stateName, int index) where TAction : FsmStateAction => fsm.GetState(stateName)!.GetAction<TAction>(index);

        /// <inheritdoc cref="GetAction{TAction}(PlayMakerFSM, string, int)"/>
        /// <param name="state">The state</param>
        /// <param name="index">The index of the action</param>

        public static TAction GetAction<TAction>(this FsmState state, int index) where TAction : FsmStateAction => state.Actions[index] as TAction;

        /// <inheritdoc cref="GetAction{TAction}(PlayMakerFSM, string, int)"/>

        public static FsmStateAction GetStateAction(this PlayMakerFSM fsm, string stateName, int index) => fsm.GetState(stateName)!.GetStateAction(index);

        /// <inheritdoc cref="GetAction{TAction}(Fsm, string, int)"/>

        public static FsmStateAction GetStateAction(this Fsm fsm, string stateName, int index) => fsm.GetState(stateName)!.GetStateAction(index);

        /// <inheritdoc cref="GetAction{TAction}(FsmState, int)"/>

        public static FsmStateAction GetStateAction(this FsmState state, int index) => state.Actions[index];

        /// <summary>
        ///     Gets first action of a given type in an FsmState.  
        /// </summary>
        /// <typeparam name="TAction">The type of actions to remove</typeparam>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state to get the actions from</param>

        public static TAction GetFirstActionOfType<TAction>(this PlayMakerFSM fsm, string stateName) where TAction : FsmStateAction => fsm.GetState(stateName)!.GetFirstActionOfType<TAction>();

        /// <inheritdoc cref="GetFirstActionOfType{TAction}(PlayMakerFSM, string)"/>

        public static TAction GetFirstActionOfType<TAction>(this Fsm fsm, string stateName) where TAction : FsmStateAction => fsm.GetState(stateName)!.GetFirstActionOfType<TAction>();

        /// <inheritdoc cref="GetFirstActionOfType{TAction}(PlayMakerFSM, string)"/>
        /// <param name="state">The fsm state</param>

        /// <summary>
        ///     Gets last action of a given type in an FsmState.  
        /// </summary>
        /// <typeparam name="TAction">The type of actions to remove</typeparam>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state to get the actions from</param>

        public static TAction GetLastActionOfType<TAction>(this PlayMakerFSM fsm, string stateName) where TAction : FsmStateAction => fsm.GetState(stateName)!.GetLastActionOfType<TAction>();

        /// <inheritdoc cref="GetLastActionOfType{TAction}(PlayMakerFSM, string)"/>

        public static TAction GetLastActionOfType<TAction>(this Fsm fsm, string stateName) where TAction : FsmStateAction => fsm.GetState(stateName)!.GetLastActionOfType<TAction>();

        /// <inheritdoc cref="GetLastActionOfType{TAction}(PlayMakerFSM, string)"/>
        /// <param name="state">The fsm state</param>

        public static TAction GetLastActionOfType<TAction>(this FsmState state) where TAction : FsmStateAction
        {
            int lastActionIndex = -1;
            for (int i = state.Actions.Length - 1; i >= 0; i--)
            {
                if (state.Actions[i] is TAction)
                {
                    lastActionIndex = i;
                    break;
                }
            }

            if (lastActionIndex == -1)
                return null;
            return state.GetAction<TAction>(lastActionIndex);
        }

        private static TVal[] AddItemToArray<TVal>(TVal[] origArray, TVal value)
        {
            TVal[] newArray = new TVal[origArray.Length + 1];
            origArray.CopyTo(newArray, 0);
            newArray[origArray.Length] = value;
            return newArray;
        }

        /// <summary>
        ///     Adds a state in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state</param>
        /// <returns>The created state</returns>

        public static void AddState(this PlayMakerFSM fsm, FsmState state)
        {
            FsmState[] states = new FsmState[fsm.FsmStates.Length + 1];

            fsm.FsmStates.CopyTo(states, 0);
            states[fsm.FsmStates.Length] = state;

            fsm.Fsm.States = states;
        }

        public static FsmState AddState(this PlayMakerFSM fsm, string name)
        {
            FsmState state = new(fsm.Fsm)
            {
                Name = name,
                Transitions = Array.Empty<FsmTransition>(),
            };
            state.ClearActions();
            AddState(fsm, state);

            return state;
        }

        public static FsmState GetState(this PlayMakerFSM fsm, string name)
        {
            return fsm.FsmStates.FirstOrDefault(s => s.Name == name);
        }

        public static void AddFirstAction(this FsmState state, FsmStateAction action)
        {
            FsmStateAction[] actions = new FsmStateAction[state.Actions.Length + 1];
            actions[0] = action;
            state.Actions.CopyTo(actions, 1);
            state.Actions = actions;
            state.actions.CopyTo(actions, 1);
            state.actions = actions;
            action.Init(state);
        }

        public static void AddLastAction(this FsmState state, FsmStateAction action)
        {
            FsmStateAction[] actions = new FsmStateAction[state.Actions.Length + 1];
            actions[state.Actions.Length] = action;
            state.Actions.CopyTo(actions, 0);
            state.Actions = actions;
            state.actions.CopyTo(actions, 0);
            state.actions = actions;
            action.Init(state);
        }

        public static void InsertAction(this FsmState state, FsmStateAction action, int index)
        {
            FsmStateAction[] actions = new FsmStateAction[state.Actions.Length + 1];
            FsmStateAction[] Actions = new FsmStateAction[state.actions.Length + 1];
            for (int i = 0; i < state.Actions.Length; i++)
            {
                if (i < index) actions[i] = state.Actions[i];
                else actions[i + 1] = state.Actions[i];
            }
            actions[index] = action;
            state.Actions = actions;
            action.Init(state);
        }

        public static void RemoveAction(this FsmState state, int index)
        {
            FsmStateAction[] actions = new FsmStateAction[state.Actions.Length - 1];
            for (int i = 0; i < state.Actions.Length - 1; i++)
            {
                if (i < index) actions[i] = state.Actions[i];
                else actions[i] = state.Actions[i + 1];
            }
            state.Actions = actions;
        }

        public static void ReplaceAction(this FsmState state, FsmStateAction action, int index)
        {
            state.Actions[index] = action;
            action.Init(state);
        }

        public static void ClearActions(this FsmState state) => state.SetActions(Array.Empty<FsmStateAction>());

        public static void SetActions(this FsmState state, params FsmStateAction[] actions)
        {
            state.Actions = actions;
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].Init(state);
            }
        }

        public static void RemoveActionsOfType<T>(this FsmState state) where T : FsmStateAction
        {
            for (int i = 0; i < state.ActionData.ActionNames.Count; i++)
            {
                if (state.ActionData.ActionNames[i] == typeof(T).FullName) //A bit hacky, but it works.
                {
                    state.RemoveAction(i);
                }
            }
        }

        public static void RemoveFirstActionOfType<T>(this FsmState state) where T : FsmStateAction
        {
            int i = Array.FindIndex<FsmStateAction>(state.Actions, a => a is T);
            if (i >= 0) state.RemoveAction(i);
        }

        public static T[] GetActionsOfType<T>(this FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().ToArray();
        }

        public static T GetFirstActionOfType<T>(this FsmState state) where T : FsmStateAction
        {
            return state.Actions.OfType<T>().FirstOrDefault();
        }

        public static FsmBool AddFsmBool(this PlayMakerFSM fsm, string name, bool value)
        {
            FsmBool fb = new FsmBool
            {
                Name = name,
                Value = value
            };

            FsmBool[] Bools = new FsmBool[fsm.FsmVariables.BoolVariables.Length + 1];
            FsmBool[] bools = new FsmBool[fsm.FsmVariables.boolVariables.Length + 1];
            fsm.FsmVariables.BoolVariables.CopyTo(Bools, 0);
            fsm.FsmVariables.boolVariables.CopyTo(bools, 0);
            bools[bools.Length - 1] = fb;
            Bools[Bools.Length - 1] = fb;
            fsm.FsmVariables.BoolVariables = Bools;
            fsm.FsmVariables.boolVariables = bools;
            return fb;
        }

        public static FsmInt AddFsmInt(this PlayMakerFSM fsm, string name, int value)
        {
            FsmInt fi = new FsmInt
            {
                Name = name,
                Value = value
            };

            FsmInt[] ints = new FsmInt[fsm.FsmVariables.IntVariables.Length + 1];
            fsm.FsmVariables.IntVariables.CopyTo(ints, 0);
            ints[ints.Length - 1] = fi;
            fsm.FsmVariables.IntVariables = ints;

            return fi;
        }

        public static FsmGameObject AddFsmGameObject(this PlayMakerFSM fsm, string name, GameObject value)
        {
            FsmGameObject fgo = new FsmGameObject
            {
                Name = name,
                Value = value
            };

            FsmGameObject[] Gos = new FsmGameObject[fsm.FsmVariables.GameObjectVariables.Length + 1];
            fsm.FsmVariables.GameObjectVariables.CopyTo(Gos, 0);
            Gos[Gos.Length - 1] = fgo;
            fsm.FsmVariables.GameObjectVariables = Gos;
            FsmGameObject[] gos = new FsmGameObject[fsm.FsmVariables.gameObjectVariables.Length + 1];
            fsm.FsmVariables.gameObjectVariables.CopyTo(gos, 0);
            gos[gos.Length - 1] = fgo;
            fsm.FsmVariables.gameObjectVariables = gos;

            return fgo;
        }

        public static FsmTransition AddTransition(this FsmState state, FsmEvent fsmEvent, FsmState toState)
        {
            FsmTransition[] transitions = new FsmTransition[state.Transitions.Length + 1];
            state.Transitions.CopyTo(transitions, 0);

            FsmTransition t = new FsmTransition
            {
                FsmEvent = fsmEvent,
                toFsmState = toState,
                toState = toState.Name,
                ToFsmState = toState,
                ToState = toState.Name
            };
            transitions[state.Transitions.Length] = t;
            state.Transitions = transitions;

            return t;
        }

        public static FsmTransition AddTransition(this FsmState state, string eventName, FsmState toState)
        {
            return state.AddTransition(FsmEvent.GetFsmEvent(eventName), toState);
        }

        public static FsmTransition AddTransition(this FsmState state, string eventName, string toState)
        {
            return state.AddTransition(eventName == "FINISHED" ? FsmEvent.Finished : FsmEvent.GetFsmEvent(eventName), state.Fsm.GetState(toState));
        }

        public static void RemoveTransitionsTo(this FsmState state, string toState)
        {
            state.Transitions = state.Transitions.Where(t => (t.ToFsmState?.Name ?? t.ToState) != toState).ToArray();
        }

        public static void RemoveTransitionsOn(this FsmState state, string eventName)
        {
            state.Transitions = state.Transitions.Where(t => t.EventName != eventName).ToArray();
        }

        public static void SetToState(this FsmTransition transition, FsmState toState)
        {
            transition.ToFsmState = toState;
            transition.toFsmState = toState;
            transition.ToState = toState.Name;
            transition.toState = toState.Name;
        }

        public static void ClearTransitions(this FsmState state)
        {
            state.Transitions = Array.Empty<FsmTransition>();
        }

        /// <summary>
        ///     Copies a state in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="fromState">The name of the state to copy</param>
        /// <param name="toState">The name of the new state</param>
        /// <returns>The new state</returns>

        public static FsmState CopyState(this PlayMakerFSM fsm, string fromState, string toState) => fsm.CopyState(fromState, toState);

        /// <inheritdoc cref="CopyState(PlayMakerFSM, string, string)"/>

        public static FsmEvent AddGlobalTransition(this PlayMakerFSM fsm, string globalEventName, string toState) => fsm.Fsm.AddGlobalTransition(globalEventName, toState);

        /// <inheritdoc cref="AddGlobalTransition(PlayMakerFSM, string, string)"/>

        public static FsmEvent AddGlobalTransition(this Fsm fsm, string globalEventName, string toState)
        {
            var ret = new FsmEvent(globalEventName) { IsGlobal = true };
            FsmTransition[] transitions = AddItemToArray(fsm.GlobalTransitions, new FsmTransition
            {
                ToState = toState,
                ToFsmState = fsm.GetState(toState),
                FsmEvent = ret,
                toState = toState,
                toFsmState = fsm.GetState(toState),
                fsmEvent = ret
            });
            fsm.GlobalTransitions = transitions;
            return ret;
        }

        /// <summary>
        ///     Adds an action in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the action is added</param>
        /// <param name="action">The action</param>

        public static void AddAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action) => fsm.GetState(stateName)!.AddAction(action);

        /// <inheritdoc cref="AddAction(PlayMakerFSM, string, FsmStateAction)"/>

        public static void AddAction(this Fsm fsm, string stateName, FsmStateAction action) => fsm.GetState(stateName)!.AddAction(action);

        /// <inheritdoc cref="AddAction(PlayMakerFSM, string, FsmStateAction)"/>
        /// <param name="state">The fsm state</param>
        /// <param name="action">The action</param>

        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            FsmStateAction[] actions = AddItemToArray(state.Actions, action);
            state.Actions = actions;
            state.actions = actions;
            action.Init(state);
        }

        /// <summary>
        ///     Adds a list of actions in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the action is added</param>
        /// <param name="actions">The actions</param>

        public static void AddActions(this PlayMakerFSM fsm, string stateName, params FsmStateAction[] actions) => fsm.GetState(stateName)!.AddActions(actions);

        /// <inheritdoc cref="AddActions(PlayMakerFSM, string, FsmStateAction[])"/>

        public static void AddActions(this Fsm fsm, string stateName, params FsmStateAction[] actions) => fsm.GetState(stateName)!.AddActions(actions);

        /// <inheritdoc cref="AddActions(PlayMakerFSM, string, FsmStateAction[])"/>
        /// <param name="state">The fsm state</param>
        /// <param name="actions">The actions</param>

        public static void AddActions(this FsmState state, params FsmStateAction[] actions)
        {
            foreach (FsmStateAction action in actions)
            {
                state.AddAction(action);
            }
        }

        /// <summary>
        ///     Adds a method in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the method is added</param>
        /// <param name="method">The method that will be invoked with the action as the parameter</param>

        public static void AddMethod(this PlayMakerFSM fsm, string stateName, Action method) => fsm.GetState(stateName)!.AddMethod(method);

        /// <inheritdoc cref="AddMethod(PlayMakerFSM, string, Action)"/>

        public static void AddMethod(this Fsm fsm, string stateName, Action method) => fsm.GetState(stateName)!.AddMethod(method);

        /// <inheritdoc cref="AddMethod(PlayMakerFSM, string, Action)"/>
        /// <param name="state">The fsm state</param>
        /// <param name="method">The method that will be invoked with the action as the parameter</param>

        public static void AddMethod(this FsmState state, Action method)
        {
            MethodAction action = new MethodAction { Method = method };
            state.AddAction(action);
        }

        /// <summary>
        ///     Adds a method with a parameter in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the method is added</param>
        /// <param name="method">The method that will be invoked with the finish action as the parameter</param>

        public static void AddLambdaMethod(this PlayMakerFSM fsm, string stateName, Action<Action> method) => fsm.GetState(stateName)!.AddLambdaMethod(method);

        /// <inheritdoc cref="AddLambdaMethod(PlayMakerFSM, string, Action{Action})"/>

        public static void AddLambdaMethod(this Fsm fsm, string stateName, Action<Action> method) => fsm.GetState(stateName)!.AddLambdaMethod(method);

        /// <inheritdoc cref="AddLambdaMethod(PlayMakerFSM, string, Action{Action})"/>
        /// <param name="state">The fsm state</param>
        /// <param name="method">The method that will be invoked with the finish action as the parameter</param>

        public static void AddLambdaMethod(this FsmState state, Action<Action> method)
        {
            FunctionAction<Action> action = new FunctionAction<Action> { Method = method };
            action.Arg = action.Finish;
            state.AddAction(action);
        }

        private static TVal[] InsertItemIntoArray<TVal>(TVal[] origArray, TVal value, int index)
        {
            int origArrayCount = origArray.Length;
            if (index < 0 || index > (origArrayCount + 1))
            {
                throw new ArgumentOutOfRangeException($"Index {index} was out of range for array with length {origArrayCount}!");
            }
            TVal[] actions = new TVal[origArrayCount + 1];
            int i;
            for (i = 0; i < index; i++)
            {
                actions[i] = origArray[i];
            }
            actions[index] = value;
            for (i = index; i < origArrayCount; i++)
            {
                actions[i + 1] = origArray[i];
            }
            return actions;
        }

        /// <summary>
        ///     Inserts an action in a PlayMakerFSM.  
        ///     Trying to insert an action out of bounds will cause a `ArgumentOutOfRangeException`.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the action is added</param>
        /// <param name="action">The action</param>
        /// <param name="index">The index to place the action in</param>
        /// <returns>bool that indicates whether the insertion was successful</returns>

        public static void InsertAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action, int index) => fsm.GetState(stateName)!.InsertAction(index, action);

        /// <inheritdoc cref="InsertAction(PlayMakerFSM, string, FsmStateAction, int)"/>

        public static void InsertAction(this PlayMakerFSM fsm, string stateName, int index, FsmStateAction action) => fsm.GetState(stateName)!.InsertAction(index, action);

        /// <inheritdoc cref="InsertAction(PlayMakerFSM, string, FsmStateAction, int)"/>

        public static void InsertAction(this Fsm fsm, string stateName, FsmStateAction action, int index) => fsm.GetState(stateName)!.InsertAction(index, action);

        /// <inheritdoc cref="InsertAction(PlayMakerFSM, string, FsmStateAction, int)"/>

        public static void InsertAction(this Fsm fsm, string stateName, int index, FsmStateAction action) => fsm.GetState(stateName)!.InsertAction(index, action);


        /// <inheritdoc cref="InsertAction(FsmState, FsmStateAction, int)"/>

        public static void InsertAction(this FsmState state, int index, FsmStateAction action)
        {
            FsmStateAction[] actions = InsertItemIntoArray(state.Actions, action, index);
            FsmStateAction[] Actions = InsertItemIntoArray(state.actions, action, index);
            state.Actions = actions;
            state.actions = Actions;
            action.Init(state);
        }

        /// <summary>
        ///     Inserts a set of actions in a PlayMakerFSM.  
        ///     Trying to insert an action out of bounds will cause a `ArgumentOutOfRangeException`.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the actions are added</param>
        /// <param name="actions">The actions</param>
        /// <param name="index">The index to place the actions in</param>
        /// <returns>bool that indicates whether the insertion was successful</returns>

        public static void InsertActions(this PlayMakerFSM fsm, string stateName, int index, params FsmStateAction[] actions) => fsm.GetState(stateName)!.InsertActions(index, actions);

        /// <inheritdoc cref="InsertActions(PlayMakerFSM, string, int, FsmStateAction[])"/>

        public static void InsertActions(this Fsm fsm, string stateName, int index, params FsmStateAction[] actions) => fsm.GetState(stateName)!.InsertActions(index, actions);

        /// <inheritdoc cref="InsertActions(PlayMakerFSM, string, int, FsmStateAction[])"/>
        /// <param name="state">The fsm state</param>
        /// <param name="actions">The actions</param>
        /// <param name="index">The index to place the actions in</param>

        public static void InsertActions(this FsmState state, int index, params FsmStateAction[] actions)
        {
            foreach (FsmStateAction action in actions)
            {
                state.InsertAction(action, index);
                index++;  // preserves order
            }
        }

        /// <summary>
        ///     Inserts a method in a PlayMakerFSM.
        ///     Trying to insert a method out of bounds will cause a `ArgumentOutOfRangeException`.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the method is added</param>
        /// <param name="method">The method that will be invoked</param>
        /// <param name="index">The index to place the action in</param>
        /// <returns>bool that indicates whether the insertion was successful</returns>

        public static void InsertMethod(this PlayMakerFSM fsm, string stateName, Action method, int index) => fsm.GetState(stateName)!.InsertMethod(index, method);

        /// <inheritdoc cref="InsertMethod(PlayMakerFSM, string, Action, int)"/>

        public static void InsertMethod(this PlayMakerFSM fsm, string stateName, int index, Action method) => fsm.GetState(stateName)!.InsertMethod(index, method);

        /// <inheritdoc cref="InsertMethod(PlayMakerFSM, string, Action, int)"/>

        public static void InsertMethod(this Fsm fsm, string stateName, Action method, int index) => fsm.GetState(stateName)!.InsertMethod(index, method);

        /// <inheritdoc cref="InsertMethod(PlayMakerFSM, string, Action, int)"/>

        public static void InsertMethod(this Fsm fsm, string stateName, int index, Action method) => fsm.GetState(stateName)!.InsertMethod(index, method);

        /// <inheritdoc cref="InsertMethod(PlayMakerFSM, string, Action, int)"/>
        /// <param name="state">The fsm state</param>
        /// <param name="method">The method that will be invoked</param>
        /// <param name="index">The index to place the action in</param>

        public static void InsertMethod(this FsmState state, Action method, int index) => state.InsertMethod(index, method);

        /// <inheritdoc cref="InsertMethod(FsmState, Action, int)"/>

        public static void InsertMethod(this FsmState state, int index, Action method)
        {
            MethodAction action = new MethodAction { Method = method };
            state.InsertAction(action, index);
        }

        /// <summary>
        ///     Inserts a method with a parameter in a PlayMakerFSM.
        ///     Trying to insert a method out of bounds will cause a `ArgumentOutOfRangeException`.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the method is added</param>
        /// <param name="method">The method that will be invoked</param>
        /// <param name="index">The index to place the action in</param>
        /// <returns>bool that indicates whether the insertion was successful</returns>

        public static void InsertLambdaMethod(this PlayMakerFSM fsm, string stateName, Action<Action> method, int index) => fsm.GetState(stateName)!.InsertLambdaMethod(index, method);

        /// <inheritdoc cref="InsertLambdaMethod(PlayMakerFSM, string, Action{Action}, int)"/>

        public static void InsertLambdaMethod(this PlayMakerFSM fsm, string stateName, int index, Action<Action> method) => fsm.GetState(stateName)!.InsertLambdaMethod(index, method);

        /// <inheritdoc cref="InsertLambdaMethod(PlayMakerFSM, string, Action{Action}, int)"/>

        public static void InsertLambdaMethod(this Fsm fsm, string stateName, Action<Action> method, int index) => fsm.GetState(stateName)!.InsertLambdaMethod(index, method);

        /// <inheritdoc cref="InsertLambdaMethod(PlayMakerFSM, string, Action{Action}, int)"/>

        public static void InsertLambdaMethod(this Fsm fsm, string stateName, int index, Action<Action> method) => fsm.GetState(stateName)!.InsertLambdaMethod(index, method);

        /// <inheritdoc cref="InsertLambdaMethod(PlayMakerFSM, string, Action{Action}, int)"/>
        /// <param name="state">The fsm state</param>
        /// <param name="method">The method that will be invoked</param>
        /// <param name="index">The index to place the action in</param>

        public static void InsertLambdaMethod(this FsmState state, Action<Action> method, int index) => state.InsertLambdaMethod(index, method);

        /// <inheritdoc cref="InsertLambdaMethod(FsmState, Action{Action}, int)"/>

        public static void InsertLambdaMethod(this FsmState state, int index, Action<Action> method)
        {
            FunctionAction<Action> action = new FunctionAction<Action> { Method = method };
            action.Arg = action.Finish;
            state.InsertAction(action, index);
        }

        /// <summary>
        /// Insert a method to run before the specified FsmStateAction.
        /// </summary>
        /// <param name="action">The action to insert before.</param>
        /// <param name="method">The method to execute.
        /// The argument will be the FsmStateAction which is being added.</param>

        public static void InsertMethodBefore(this FsmStateAction action, Action method)
        {
            FsmState state = action.State;
            int idx = Array.IndexOf(state.Actions, action);
            state.InsertMethod(idx, method);
        }

        /// <summary>
        /// Insert a method to run after the specified FsmStateAction.
        /// </summary>
        /// <param name="action">The action to insert after.</param>
        /// <param name="method">The method to execute.
        /// The argument will be the FsmStateAction which is being added.</param>

        public static void InsertMethodAfter(this FsmStateAction action, Action method)
        {
            FsmState state = action.State;
            int idx = Array.IndexOf(state.Actions, action);
            state.InsertMethod(idx + 1, method);
        }

        /// <summary>
        /// Insert an action to run before the specified FsmStateAction.
        /// </summary>
        /// <param name="action">The action to insert before.</param>
        /// <param name="newAction">The action to add.</param>

        public static void InsertActionBefore(this FsmStateAction action, FsmStateAction newAction)
        {
            FsmState state = action.State;
            int idx = Array.IndexOf(state.Actions, action);
            state.InsertAction(idx, newAction);
        }

        /// <summary>
        /// Insert an action to run after the specified FsmStateAction.
        /// </summary>
        /// <param name="action">The action to insert after.</param>
        /// <param name="newAction">The action to add.</param>

        public static void InsertActionAfter(this FsmStateAction action, FsmStateAction newAction)
        {
            FsmState state = action.State;
            int idx = Array.IndexOf(state.Actions, action);
            state.InsertAction(idx + 1, newAction);
        }
        /// <summary>
        ///     Replaces an action in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the action is replaced</param>
        /// <param name="action">The action</param>
        /// <param name="index">The index of the action</param>

        public static void ReplaceAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action, int index) => fsm.GetState(stateName)!.ReplaceAction(index, action);

        /// <inheritdoc cref="ReplaceAction(PlayMakerFSM, string, FsmStateAction, int)"/>

        public static void ReplaceAction(this PlayMakerFSM fsm, string stateName, int index, FsmStateAction action) => fsm.GetState(stateName)!.ReplaceAction(index, action);

        /// <inheritdoc cref="ReplaceAction(PlayMakerFSM, string, FsmStateAction, int)"/>

        public static void ReplaceAction(this Fsm fsm, string stateName, FsmStateAction action, int index) => fsm.GetState(stateName)!.ReplaceAction(index, action);

        /// <inheritdoc cref="ReplaceAction(PlayMakerFSM, string, FsmStateAction, int)"/>

        public static void ReplaceAction(this Fsm fsm, string stateName, int index, FsmStateAction action) => fsm.GetState(stateName)!.ReplaceAction(index, action);

        /// <inheritdoc cref="ReplaceAction(FsmState, FsmStateAction, int)"/>

        public static void ReplaceAction(this FsmState state, int index, FsmStateAction action)
        {
            state.Actions[index] = action;
            state.actions[index] = action;
            action.Init(state);
        }

        /// <summary>
        ///     Replaces all actions in a PlayMakerFSM state.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state in which the actions are to be replaced</param>
        /// <param name="actions">The new actions of the state</param>

        public static void ReplaceAllActions(this PlayMakerFSM fsm, string stateName, params FsmStateAction[] actions) => fsm.GetState(stateName)!.ReplaceAllActions(actions);

        /// <inheritdoc cref="ReplaceAllActions(PlayMakerFSM, string, FsmStateAction[])"/>

        public static void ReplaceAllActions(this Fsm fsm, string stateName, params FsmStateAction[] actions) => fsm.GetState(stateName)!.ReplaceAllActions(actions);

        /// <inheritdoc cref="ReplaceAllActions(PlayMakerFSM, string, FsmStateAction[])"/>
        /// <param name="state">The fsm state</param>
        /// <param name="actions">The action</param>

        public static void ReplaceAllActions(this FsmState state, params FsmStateAction[] actions)
        {
            state.Actions = actions;
            foreach (FsmStateAction action in actions)
            {
                action.Init(state);
            }
        }
        /// <summary>
        ///     Changes a transition endpoint in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state from which the transition starts</param>
        /// <param name="eventName">The event of the transition</param>
        /// <param name="toState">The new endpoint of the transition</param>
        /// <returns>bool that indicates whether the change was successful</returns>

        public static bool ChangeTransition(this PlayMakerFSM fsm, string stateName, string eventName, string toState) => fsm.GetState(stateName)!.ChangeTransition(eventName, toState);

        /// <inheritdoc cref="ChangeTransition(PlayMakerFSM, string, string, string)"/>

        public static bool ChangeTransition(this Fsm fsm, string stateName, string eventName, string toState) => fsm.GetState(stateName)!.ChangeTransition(eventName, toState);

        /// <inheritdoc cref="ChangeTransition(PlayMakerFSM, string, string, string)"/>
        /// <param name="state">The fsm state</param>
        /// <param name="eventName">The event of the transition</param>
        /// <param name="toState">The new endpoint of the transition</param>

        public static bool ChangeTransition(this FsmState state, string eventName, string toState)
        {
            if (state == null) {
                return false;
            }
            FsmTransition[] transitions = state.transitions;
            FsmTransition[] Transitions = state.Transitions;
            FsmTransition transition = transitions[0];
            FsmTransition Transition = Transitions[0];
            for (int i = 0; i < Transitions.Length; i++)
            {
                transition = transitions[i];
                Transition = Transitions[i];
                if (transition.EventName == "FINISHED")
                {
                    transition.ToState = "SLOT 2";
                    transition.ToFsmState = state.fsm.GetState("SLOT 2");
                    transition.toState = "SLOT 2";
                    transition.toFsmState = state.fsm.GetState("SLOT 2");
                    Transition.ToState = "SLOT 2";
                    Transition.ToFsmState = state.fsm.GetState("SLOT 2");
                    Transition.toState = "SLOT 2";
                    Transition.toFsmState = state.fsm.GetState("SLOT 2");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Changes a global transition in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="globalEventName">The name of transition event</param>
        /// <param name="toState">The name of the new state</param>
        /// <returns>bool that indicates whether the change was successful</returns>

        public static bool ChangeGlobalTransition(this PlayMakerFSM fsm, string globalEventName, string toState) => fsm.Fsm.ChangeGlobalTransition(globalEventName, toState);

        /// <inheritdoc cref="ChangeGlobalTransition(PlayMakerFSM, string, string)"/>

        public static bool ChangeGlobalTransition(this Fsm fsm, string globalEventName, string toState)
        {
            var transition = fsm.GetGlobalTransition(globalEventName);
            if (transition == null)
            {
                return false;
            }
            transition.ToState = toState;
            transition.ToFsmState = fsm.GetState(toState);
            transition.toState = toState;
            transition.toFsmState = fsm.GetState(toState);
            return true;
        }

        private static TVal[] RemoveItemsFromArray<TVal>(TVal[] origArray, Func<TVal, bool> shouldBeRemovedCallback)
        {
            int amountOfRemoved = 0;
            foreach (TVal tmpValue in origArray)
            {
                if (shouldBeRemovedCallback(tmpValue))
                {
                    amountOfRemoved++;
                }
            }
            if (amountOfRemoved == 0)
            {
                return origArray;
            }
            TVal[] newArray = new TVal[origArray.Length - amountOfRemoved];
            for (int i = origArray.Length - 1; i >= 0; i--)
            {
                TVal tmpValue = origArray[i];
                if (shouldBeRemovedCallback(tmpValue))
                {
                    amountOfRemoved--;
                    continue;
                }
                newArray[i - amountOfRemoved] = tmpValue;
            }
            return newArray;
        }

        /// <summary>
        ///     Removes a state in a PlayMakerFSM.  
        ///     Trying to remove a state that doesn't exist will result in the states not being changed.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state to remove</param>

        public static void RemoveState(this PlayMakerFSM fsm, string stateName) => fsm.Fsm.RemoveState(stateName);

        /// <inheritdoc cref="RemoveState(PlayMakerFSM, string)"/>

        public static void RemoveState(this Fsm fsm, string stateName) => fsm.States = RemoveItemsFromArray<FsmState>(fsm.States, x => x.Name == stateName);

        /// <summary>
        ///     Removes a transition in a PlayMakerFSM.  
        ///     Trying to remove a transition that doesn't exist will result in the transitions not being changed.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state from which the transition starts</param>
        /// <param name="eventName">The event of the transition</param>

        /// <inheritdoc cref="RemoveTransition(PlayMakerFSM, string, string)"/>

        public static void RemoveTransition(this Fsm fsm, string stateName, string eventName) => fsm.GetState(stateName)!.RemoveTransition(eventName);

        /// <inheritdoc cref="RemoveTransition(PlayMakerFSM, string, string)"/>
        /// <param name="state">The fsm state</param>
        /// <param name="eventName">The event of the transition</param>

        public static void RemoveTransition(this FsmState state, string eventName) => state.Transitions = RemoveItemsFromArray<FsmTransition>(state.Transitions, x => x.EventName == eventName);

        /// <summary>
        ///     Removes a global transition in a PlayMakerFSM.  
        ///     Trying to remove a transition that doesn't exist will result in the transitions not being changed.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="globalEventName">The event of the transition</param>

        public static void RemoveGlobalTransition(this PlayMakerFSM fsm, string globalEventName) => fsm.Fsm.RemoveGlobalTransition(globalEventName);

        /// <inheritdoc cref="RemoveGlobalTransition(PlayMakerFSM, string)"/>

        public static void RemoveGlobalTransition(this Fsm fsm, string globalEventName)
        {
            fsm.GlobalTransitions = RemoveItemsFromArray<FsmTransition>(fsm.GlobalTransitions, x => x.EventName == globalEventName);
        }

        /// <summary>
        ///     Removes all transitions to a specified transition in a PlayMakerFSM.  
        ///     Trying to remove a transition that doesn't exist will result in the transitions not being changed.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="toState">The target of the transition</param>

        public static void RemoveTransitionsTo(this PlayMakerFSM fsm, string toState) => fsm.Fsm.RemoveTransitionsTo(toState);

        /// <inheritdoc cref="RemoveTransitionsTo(PlayMakerFSM, string)"/>

        public static void RemoveTransitionsTo(this Fsm fsm, string toState)
        {
            foreach (FsmState state in fsm.States)
            {
                state.RemoveTransitionsTo(toState);
            }
        }

        /// <summary>
        ///     Removes all transitions from a state to another specified state in a PlayMakerFSM.  
        ///     Trying to remove a transition that doesn't exist will result in the transitions not being changed.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state from which the transition starts</param>
        /// <param name="toState">The target of the transition</param>

        public static void RemoveTransitionsTo(this PlayMakerFSM fsm, string stateName, string toState) => fsm.GetState(stateName)!.RemoveTransitionsTo(toState);

        /// <inheritdoc cref="RemoveTransitionsTo(PlayMakerFSM, string, string)"/>

        public static void RemoveTransitionsTo(this Fsm fsm, string stateName, string toState) => fsm.GetState(stateName)!.RemoveTransitionsTo(toState);

        /// <summary>
        ///     Removes all transitions from a state in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state from which the transition starts</param>

        public static void RemoveTransitions(this PlayMakerFSM fsm, string stateName) => fsm.GetState(stateName)!.RemoveTransitions();

        /// <inheritdoc cref="RemoveTransitions(PlayMakerFSM, string)"/>

        public static void RemoveTransitions(this Fsm fsm, string stateName) => fsm.GetState(stateName)!.RemoveTransitions();

        /// <inheritdoc cref="RemoveTransitions(PlayMakerFSM, string)"/>
        /// <param name="state">The fsm state</param>

        public static void RemoveTransitions(this FsmState state)
        {
            state.Transitions = new Il2CppReferenceArray<FsmTransition>([]);
        }


    

        /// <summary>
        ///     Removes all actions of a given type in a PlayMakerFSM.
        /// </summary>
        /// <typeparam name="TAction">The type of actions to remove</typeparam>
        /// <param name="fsm">The fsm</param>

        public static void RemoveActionsOfType<TAction>(this PlayMakerFSM fsm) => fsm.RemoveActionsOfType<TAction>();

        /// <summary>
        ///     Removes last action of a given type in an FsmState.  
        /// </summary>
        /// <typeparam name="TAction">The type of actions to remove</typeparam>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state to remove the actions from</param>

        public static void RemoveLastActionOfType<TAction>(this PlayMakerFSM fsm, string stateName) => fsm.GetState(stateName)!.RemoveLastActionOfType<TAction>();

        /// <inheritdoc cref="RemoveLastActionOfType{TAction}(PlayMakerFSM, string)"/>

        public static void RemoveLastActionOfType<TAction>(this Fsm fsm, string stateName) => fsm.GetState(stateName)!.RemoveLastActionOfType<TAction>();

        /// <inheritdoc cref="RemoveLastActionOfType{TAction}(PlayMakerFSM, string)"/>
        /// <param name="state">The fsm state</param>

        public static void RemoveLastActionOfType<TAction>(this FsmState state)
        {
            int lastActionIndex = -1;
            for (int i = state.ActionData.ActionNames.Count - 1; i >= 0; i--)
            {
                if (state.ActionData.ActionNames[i] == typeof(TAction).FullName)
                {
                    lastActionIndex = i;
                    break;
                }
            }

            if (lastActionIndex == -1)
                return;
            state.RemoveAction(lastActionIndex);
        }

        /// <summary>
        ///     Disables an action in a PlayMakerFSM.  
        ///     Trying to disable an action that doesn't exist will result in the actions not being changed.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state with the action</param>
        /// <param name="index">The index of the action</param>
        /// <returns>bool that indicates whether the disabling was successful</returns>

        public static bool DisableAction(this PlayMakerFSM fsm, string stateName, int index) => fsm.GetState(stateName)!.DisableAction(index);

        /// <inheritdoc cref="DisableAction(PlayMakerFSM, string, int)"/>

        public static bool DisableAction(this Fsm fsm, string stateName, int index) => fsm.GetState(stateName)!.DisableAction(index);

        /// <inheritdoc cref="DisableAction(PlayMakerFSM, string, int)"/>
        /// <param name="state">The fsm state</param>
        /// <param name="index">The index of the action</param>

        public static bool DisableAction(this FsmState state, int index)
        {
            if (index < 0 || index >= state.Actions.Length)
            {
                return false;
            }
            state.Actions[index].Enabled = false;
            return true;
        }

        /// <summary>
        ///     Disables a set of actions in a PlayMakerFSM.  
        ///     Trying to disable an action that doesn't exist will result in the actions not being changed.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state with the action</param>
        /// <param name="indices">The indices of the action</param>
        /// <returns>bool that indicates whether all the disablings were successful</returns>

        public static bool DisableActions(this PlayMakerFSM fsm, string stateName, params int[] indices) => fsm.GetState(stateName)!.DisableActions(indices);

        /// <inheritdoc cref="DisableActions(PlayMakerFSM, string, int[])"/>

        public static bool DisableActions(this Fsm fsm, string stateName, params int[] indices) => fsm.GetState(stateName)!.DisableActions(indices);

        /// <inheritdoc cref="DisableActions(PlayMakerFSM, string, int[])"/>
        /// <param name="state">The fsm state</param>
        /// <param name="indices">The indices of the action</param>

        public static bool DisableActions(this FsmState state, params int[] indices)
        {
            bool ret = true;
            foreach (int index in indices)
            {
                ret = ret && state.DisableAction(index);
            }
            return ret;
        }

        /// <summary>
        ///     Disables all actions of a given type in a PlayMakerFSM.
        /// </summary>
        /// <typeparam name="TAction">The type of actions to disable</typeparam>
        /// <param name="fsm">The fsm</param>

        public static void DisableActionsOfType<TAction>(this PlayMakerFSM fsm) => fsm.Fsm.DisableActionsOfType<TAction>();

        /// <inheritdoc cref="DisableActionsOfType{TAction}(PlayMakerFSM)"/>

        public static void DisableActionsOfType<TAction>(this Fsm fsm)
        {
            foreach (FsmState state in fsm.States)
            {
                state.DisableActionsOfType<TAction>();
            }
        }

        /// <summary>
        ///     Disables all actions of a given type in an FsmState.  
        /// </summary>
        /// <typeparam name="TAction">The type of actions to disable</typeparam>
        /// <param name="fsm">The fsm</param>
        /// <param name="stateName">The name of the state to disable the actions from</param>

        public static void DisableActionsOfType<TAction>(this PlayMakerFSM fsm, string stateName) => fsm.GetState(stateName)!.DisableActionsOfType<TAction>();

        /// <inheritdoc cref="DisableActionsOfType{TAction}(PlayMakerFSM, string)"/>

        public static void DisableActionsOfType<TAction>(this Fsm fsm, string stateName) => fsm.GetState(stateName)!.DisableActionsOfType<TAction>();

        /// <inheritdoc cref="DisableActionsOfType{TAction}(PlayMakerFSM, string)"/>
        /// <param name="state">The fsm state</param>

        public static void DisableActionsOfType<TAction>(this FsmState state)
        {
            int i = 0;
            foreach (string actionName in state.ActionData.ActionNames)
            {
                if (actionName == typeof(TAction).FullName)
                {
                    state.DisableAction(i);
                }
                i++;
            }
        }

        private static TVar[] MakeNewVariableArray<TVar>(TVar[] orig, string name) where TVar : NamedVariable, new()
        {
            TVar[] newArray = new TVar[orig.Length + 1];
            orig.CopyTo(newArray, 0);
            newArray[orig.Length] = new TVar { Name = name };
            return newArray;
        }

        /// <summary>
        ///     Adds a fsm variable in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="name">The name of the new variable</param>
        /// <returns>The newly created variable</returns>

        public static FsmFloat AddFloatVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddFloatVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmFloat AddFloatVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmFloat>(fsm.Variables.FloatVariables, name);
            fsm.Variables.FloatVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmInt AddIntVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddIntVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmInt AddIntVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmInt>(fsm.Variables.IntVariables, name);
            fsm.Variables.IntVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmBool AddBoolVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddBoolVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmBool AddBoolVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmBool>(fsm.Variables.BoolVariables, name);
            fsm.Variables.BoolVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmString AddStringVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddStringVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmString AddStringVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmString>(fsm.Variables.StringVariables, name);
            fsm.Variables.StringVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector2 AddVector2Variable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddVector2Variable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector2 AddVector2Variable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmVector2>(fsm.Variables.Vector2Variables, name);
            fsm.Variables.Vector2Variables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector3 AddVector3Variable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddVector3Variable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector3 AddVector3Variable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmVector3>(fsm.Variables.Vector3Variables, name);
            fsm.Variables.Vector3Variables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmColor AddColorVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddColorVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmColor AddColorVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmColor>(fsm.Variables.ColorVariables, name);
            fsm.Variables.ColorVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmRect AddRectVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddRectVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmRect AddRectVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmRect>(fsm.Variables.RectVariables, name);
            fsm.Variables.RectVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmQuaternion AddQuaternionVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddQuaternionVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmQuaternion AddQuaternionVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmQuaternion>(fsm.Variables.QuaternionVariables, name);
            fsm.Variables.QuaternionVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmGameObject AddGameObjectVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.AddGameObjectVariable(name);

        /// <inheritdoc cref="AddFloatVariable(PlayMakerFSM, string)"/>

        public static FsmGameObject AddGameObjectVariable(this Fsm fsm, string name)
        {
            var tmp = MakeNewVariableArray<FsmGameObject>(fsm.Variables.GameObjectVariables, name);
            fsm.Variables.GameObjectVariables = tmp;
            return tmp[tmp.Length - 1];
        }

        private static TVar FindInVariableArray<TVar>(TVar[] orig, string name) where TVar : NamedVariable, new()
        {
            foreach (TVar item in orig)
            {
                if (item.Name == name)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        ///     Finds a fsm variable in a PlayMakerFSM.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="name">The name of the variable</param>
        /// <returns>The variable, null if not found</returns>

        public static FsmFloat FindFloatVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindFloatVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmFloat FindFloatVariable(this Fsm fsm, string name) => FindInVariableArray<FsmFloat>(fsm.Variables.FloatVariables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmInt FindIntVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindIntVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmInt FindIntVariable(this Fsm fsm, string name) => FindInVariableArray<FsmInt>(fsm.Variables.IntVariables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmBool FindBoolVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindBoolVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmBool FindBoolVariable(this Fsm fsm, string name) => FindInVariableArray<FsmBool>(fsm.Variables.BoolVariables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmString FindStringVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindStringVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmString FindStringVariable(this Fsm fsm, string name) => FindInVariableArray<FsmString>(fsm.Variables.StringVariables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector2 FindVector2Variable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindVector2Variable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector2 FindVector2Variable(this Fsm fsm, string name) => FindInVariableArray<FsmVector2>(fsm.Variables.Vector2Variables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector3 FindVector3Variable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindVector3Variable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector3 FindVector3Variable(this Fsm fsm, string name) => FindInVariableArray<FsmVector3>(fsm.Variables.Vector3Variables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmColor FindColorVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindColorVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmColor FindColorVariable(this Fsm fsm, string name) => FindInVariableArray<FsmColor>(fsm.Variables.ColorVariables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmRect FindRectVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindRectVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmRect FindRectVariable(this Fsm fsm, string name) => FindInVariableArray<FsmRect>(fsm.Variables.RectVariables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmQuaternion FindQuaternionVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindQuaternionVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmQuaternion FindQuaternionVariable(this Fsm fsm, string name) => FindInVariableArray<FsmQuaternion>(fsm.Variables.QuaternionVariables, name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmGameObject FindGameObjectVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.FindGameObjectVariable(name);

        /// <inheritdoc cref="FindFloatVariable(PlayMakerFSM, string)"/>

        public static FsmGameObject FindGameObjectVariable(this Fsm fsm, string name) => FindInVariableArray<FsmGameObject>(fsm.Variables.GameObjectVariables, name);

        /// <summary>
        ///     Gets a fsm variable in a PlayMakerFSM. Creates a new one if none with the name are present.
        /// </summary>
        /// <param name="fsm">The fsm</param>
        /// <param name="name">The name of the variable</param>
        /// <returns>The variable</returns>

        public static FsmFloat GetFloatVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetFloatVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmFloat GetFloatVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindFloatVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddFloatVariable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmInt GetIntVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetIntVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmInt GetIntVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindIntVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddIntVariable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmBool GetBoolVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetBoolVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmBool GetBoolVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindBoolVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddBoolVariable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmString GetStringVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetStringVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmString GetStringVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindStringVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddStringVariable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector2 GetVector2Variable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetVector2Variable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector2 GetVector2Variable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindVector2Variable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddVector2Variable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector3 GetVector3Variable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetVector3Variable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmVector3 GetVector3Variable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindVector3Variable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddVector3Variable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmColor GetColorVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetColorVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmColor GetColorVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindColorVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddColorVariable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmRect GetRectVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetRectVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmRect GetRectVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindRectVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddRectVariable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmQuaternion GetQuaternionVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetQuaternionVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmQuaternion GetQuaternionVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindQuaternionVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddQuaternionVariable(name);
        }

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmGameObject GetGameObjectVariable(this PlayMakerFSM fsm, string name) => fsm.Fsm.GetGameObjectVariable(name);

        /// <inheritdoc cref="GetFloatVariable(PlayMakerFSM, string)"/>

        public static FsmGameObject GetGameObjectVariable(this Fsm fsm, string name)
        {
            var tmp = fsm.FindGameObjectVariable(name);
            if (tmp != null)
                return tmp;
            return fsm.AddGameObjectVariable(name);
        }

    }
}
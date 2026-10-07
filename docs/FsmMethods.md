# FsmMethods

## General Info

This is handles executing custom code from an FSMAction. While it is possible to run at certain timings in FSMs based on event triggers this prevents the FSM from continuing until the intended code has been run which often leads to more consistent behavior. 

This is our main way of adding hooks to game events that do not have existing and easily utilizable hooks.

## Custom Fsm Method Manager

This class handles the registration and helper function related to the custom code that is intended to be run from an FSM. For functions that are statically generated, they are defined in the RegisteredCustomFsmMethods Dictionary. Others are dynamically added at runtime because they are more generalized for a set of common actions.

## Custom Fsm Methods

This is a monobehaviour that handles the invocation of the custom code. RunCustomMethod is essentially a wrapper function to define a static method that can run any Registered Custom Method given the name exists. This is necessary as the FsmAction used to call this function is very limited on what number and types parameters can be defined. As such Parameters are defined on the end of the action.

## Registered Fsm Methods

This abstract class defines the structure of our custom method. `OnCalled()` is what code is run whenever the action is called via the FSM.

The optional `OnRegister()` defines any initialization that should be done on the method being registered.

The option `Update()` defines any code that should be run just before the OnCalled is called, or any other time this method's `Update()` method is called directly. This is useful for grabbing/updating any variables that are used as params or need updating at other timings.

The bool variable UpdateBeforeRun defines whether the `Update()` function should be run before `OnCalled` or if it should only run on manual invocation.
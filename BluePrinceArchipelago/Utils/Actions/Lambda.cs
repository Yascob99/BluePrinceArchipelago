using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//From Hollow Knight ItemChanger https://github.com/homothetyhk/HollowKnight.ItemChanger/blob/8f9b80b6ff838c581f2c45e421fdc688dc6a3f3d/ItemChanger/FsmStateActions/Lambda.cs

namespace BluePrinceArchipelago.Utils.Actions
{
    /// <summary>
    /// FsmStateAction which invokes a delegate.
    /// </summary>
    public class Lambda : FsmStateAction
    {
        private readonly Action _method;

        public Lambda(Action method)
        {
            _method = method;
        }

        public override void OnEnter()
        {
            try
            {
                _method();
            }
            catch (Exception e)
            {
                LogError($"Error in FsmStateAction Lambda in {this.Fsm.FsmComponent.gameObject.name} - {this.Fsm.FsmComponent.FsmName}:\n{e}");
            }

            Finish();
        }
    }
}

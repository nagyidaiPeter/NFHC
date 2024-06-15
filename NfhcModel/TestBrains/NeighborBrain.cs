using NFH.Game;
using NFH.Game.Logic;
using System;

namespace NfhcModel.TestBrains
{
    public class NeighborBrain : NeighborNFH1
    {
        [NonSerialized]
        private Item theSofa;
        [NonSerialized]
        private Item theSofaWithFartBag;
        [NonSerialized]
        private Item theBinoculars;
        [NonSerialized]
        private Item theBinocularsGlue;
        [NonSerialized]
        private Item theWindow;
        [NonSerialized]
        private int currentSofaSitCycle;

        public override bool Run(ActorBrain theBrain)
        {
            if (this.antennaTwistedEvent.CheckAndReset())
            {
                this.postRepairAntennaState = this.GetState();
                var state = new State(DiscoverTwistedAntennaState);
                SetState(state);
            }
            if (this.microwaveDirtyEvent.CheckAndReset())
            {
                this.postRepairMicroWaveState = this.GetState();
                this.SetState(new State(GoToDirtyMicrowaveState));
            }
            return base.Run(theBrain);
        }

        protected override void InitState(ActorBrain theBrain)
        {
            base.InitState(theBrain);
            this.AssignCachedItems();
            this.SetState(new BrainScriptBase.State(this.GoToSofaState));
        }

        private void GoToSofaState(ActorBrain theBrain)
        {
            this.logic.SetActorBubble(theBrain, this.theSofa.Icon);
            Item firstExistingEntity = BrainScriptBase.GetFirstExistingEntity((GameEntity)this.theSofa, (GameEntity)this.theSofaWithFartBag) as Item;
            if (!BrainScriptBase.EnteringObject(theBrain, (GameEntity)firstExistingEntity))
            {
                ActionScript actionScript1 = new ActionScript(theBrain);
                if (firstExistingEntity == theSofaWithFartBag)
                {
                    this.logic.SetActorBubble(theBrain, this.theSofaWithFartBag.Icon);
                    ActionScript actionScript2 = new ActionScript(theBrain);
                    actionScript2.AddAction((GameEntity)this.theSofaWithFartBag, "surprise");
                    actionScript2.AddLeave((GameEntity)this.theSofaWithFartBag);
                    actionScript2.AddShout(ShoutCategory.Heavy);
                    actionScript2.AddMakeTrick();
                    actionScript2.AddSwitchObject((GameEntity)this.theSofaWithFartBag, theSofa);
                    this.currentSofaSitCycle = 0;
                    actionScript2.Execute(new BrainScriptBase.State(this.GoToSofaState));
                }
                else
                {
                    bool tvWatching = currentSofaSitCycle < 6;
                    if (this.currentSofaSitCycle % 2 == 0 && tvWatching)
                    {
                        actionScript1.AddStoppableAction((GameEntity)this.theSofa, "sit_remo");
                        this.logic.SetActorBubble(theBrain, this.theSofa.Icon);
                    }
                    else if (tvWatching)
                    {
                        actionScript1.AddStoppableAction((GameEntity)this.theSofa, "sit");
                        this.logic.SetActorBubble(theBrain, this.theSofa.Icon);
                    }

                    UnityEngine.Debug.Log("tvWatching " + tvWatching);

                    if (!tvWatching)
                    {
                        UnityEngine.Debug.Log("Switching to GoToBinocularsState");
                        this.SetState(new State(GoToBinocularsState));
                    }
                    else
                    {
                        actionScript1.Execute();
                    }
                    currentSofaSitCycle++;
                }
            }
            else
                this.currentSofaSitCycle = 0;
        }

        private void GoToBinocularsState(ActorBrain theBrain)
        {
            this.logic.SetActorBubble(theBrain, this.theBinoculars.Icon);
            Item firstExistingEntity = BrainScriptBase.GetFirstExistingEntity((GameEntity)this.theBinoculars, (GameEntity)this.theBinocularsGlue, (GameEntity)this.theWindow) as Item;
            if (BrainScriptBase.MovingToObject(theBrain, (GameEntity)firstExistingEntity))
                return;

            ActionScript actionScript = new ActionScript(theBrain);

            if (BrainScriptBase.IsEntityPresent((GameEntity)this.theBinocularsGlue))
            {
                actionScript.AddAction((GameEntity)this.theBinocularsGlue, "peep_glue");
                actionScript.AddShout(ShoutCategory.Medium);
                actionScript.AddRemoveObject((GameEntity)this.theBinocularsGlue);
                actionScript.AddMakeTrick();
            }
            else if (BrainScriptBase.IsEntityPresent((GameEntity)this.theBinoculars))
            {
                actionScript.AddAction((GameEntity)this.theBinoculars, "peep");
            }
            else
            {
                actionScript.AddAction((GameEntity)this.theWindow, "peep_no_binoculars");
            }

            actionScript.Execute(new BrainScriptBase.State(this.GoToSofaState));
        }

        private void AssignCachedItems()
        {
            if ((UnityEngine.Object)this.theSofa == (UnityEngine.Object)null)
                this.theSofa = this.logic.GetItemByName("lir/sofa");
            if ((UnityEngine.Object)this.theSofaWithFartBag == (UnityEngine.Object)null)
                this.theSofaWithFartBag = this.logic.GetItemByName("lir/sofa_fartbag");
            if ((UnityEngine.Object)this.theBinoculars == (UnityEngine.Object)null)
                this.theBinoculars = this.logic.GetItemByName("kit/binoculars");
            if ((UnityEngine.Object)this.theBinocularsGlue == (UnityEngine.Object)null)
                this.theBinocularsGlue = this.logic.GetItemByName("kit/binoculars_glue");
            if (!((UnityEngine.Object)this.theWindow == (UnityEngine.Object)null))
                return;
            this.theWindow = this.logic.GetItemByName("kit/window");
        }
    }
}

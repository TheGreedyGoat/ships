class NPCFlee : NPCBehaviour
{
    protected override BehaviourState GetNextState()
    {
        return BehaviourState.Disengage;
    }
}

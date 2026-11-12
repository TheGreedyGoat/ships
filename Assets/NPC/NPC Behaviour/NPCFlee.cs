class NPCFlee : NPCBehaviour
{
    protected override BehaviourState UpdateState()
    {
        return BehaviourState.Disengage;
    }
}

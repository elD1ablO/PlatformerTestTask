using PlayerControl;

namespace LevelComponents
{
    public class SpikeTrapComponent : BasePlayerTriggerComponent
    {
        protected override void OnPlayerEnterAction(IPlayerObject playerObject)
        {
            playerObject.KillZoneEntered();
        }
    }
}

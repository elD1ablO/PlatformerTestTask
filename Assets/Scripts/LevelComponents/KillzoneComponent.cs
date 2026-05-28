using PlayerControl;

namespace LevelComponents
{
    public class KillzoneComponent : BasePlayerTriggerComponent
    {
        protected override void OnPlayerEnterAction(IPlayerObject playerObject)
        {
            playerObject.KillZoneEntered();
        }
    }
}

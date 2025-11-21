using EFT;

namespace SAIN.Components.PlayerComponentSpace.PersonClasses;

public class ProfileData
{
    public string ProfileId { get; }
    public string Nickname { get; }
    public EPlayerSide Side { get; }

    public ProfileData(Player player)
    {
        ProfileId = player.ProfileId;
        Nickname = player.Profile.Nickname;
        Side = player.Profile.Side;
    }
}
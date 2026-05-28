using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Sounds Collection")]
public class SoundsCollectionSO : ScriptableObject
{
    [Header("Music")]
    public SoundSO[] MenuMusic;
    public SoundSO[] GameMusic;

    [Header("Player Sounds")]
    public SoundSO[] Walk;
    public SoundSO[] Jump;
    public SoundSO[] Land;
    public SoundSO[] HeadHit;

    [Header("Environment Sounds")]
    public SoundSO[] WallHit;
    public SoundSO[] BonusCubeHit;
    public SoundSO[] BonusEffect;
}

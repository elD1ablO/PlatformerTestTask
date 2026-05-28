using UnityEngine;

[CreateAssetMenu(menuName = "Audio/SoundSO")]
public class SoundSO : ScriptableObject
{
    public enum AutioTypes
    {
        SFX,
        Music
    }
    public AutioTypes audioType;
    public AudioClip audioClip;
    public bool Loop = false;
    public bool RandomizePitch = false;
    [Range(0f, 1f)]
    public float RandomPitchModifier = 0.1f;
    [Range(0f, 2f)]
    public float Volume = 1f;
    [Range(0f, 3f)]
    public float Pitch = 1f;
}

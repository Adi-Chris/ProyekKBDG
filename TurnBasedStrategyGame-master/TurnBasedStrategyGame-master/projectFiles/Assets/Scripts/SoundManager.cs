using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] AudioSource sfxAudioSource;

    [Header("Unit Select")]
    [SerializeField] AudioClip humanSelectSFX;
    // Skele unit tidak ada select, karena AI langsung menggerakkan unit

    [Header("Unit Moving")]
    [SerializeField] AudioClip humanMoveSFX;
    [SerializeField] AudioClip skeleMoveSFX;

    [Header("Unit Attack Select")]
    [SerializeField] AudioClip meleeAttackSelectSFX;
    [SerializeField] AudioClip rangedAttackSelectSFX;

    [Header("Unit Attack")]
    [SerializeField] AudioClip meleeAttackSFX;
    [SerializeField] AudioClip rangedAttackSFX;

    [Header("Human Unit Death")]
    [SerializeField] AudioClip humanGigaMungusDeathSFX;
    [SerializeField] AudioClip humanSoldierDeathSFX;
    [SerializeField] AudioClip humanArcherDeathSFX;
    [SerializeField] AudioClip humanBaldArcherDeathSFX;

    [Header("Skele Unit Death")]
    [SerializeField] AudioClip skeleGigaMungusDeathSFX;
    [SerializeField] AudioClip skeleSoldierDeathSFX;
    [SerializeField] AudioClip skeleArcherDeathSFX;
    [SerializeField] AudioClip skeleBaldArcherDeathSFX;

    [Header("Other SFX")]
    [SerializeField] AudioClip cancelMoveSFX;

    [Header("Condition SFX")]
    [SerializeField] AudioClip gameWinSFX;
    [SerializeField] AudioClip gameLoseSFX;

    [Header("UI SFX")]
    [SerializeField] AudioClip buttonHoverSFX;
    [SerializeField] AudioClip buttonClickSFX;

    // Audio Sources
    public void PlaySFX(AudioClip audioClip) {
        sfxAudioSource.PlayOneShot(audioClip);
    }

    // Function to fade out BGM volume over specified duration
    public void FadeOutBGM(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmAudioSource.volume;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            bgmAudioSource.volume = Mathf.Lerp(startVolume, 0f, (Time.time - startTime) / duration);
            yield return null;
        }

        bgmAudioSource.volume = 0f; // Ensure volume is completely faded out
        bgmAudioSource.Stop(); // Stop the audio after fading out (optional)
    }

    // Unit Select
    public void PlayHumanSelectSFX() {
        sfxAudioSource.PlayOneShot(humanSelectSFX);
    }

    // Unit Moving
    public void PlayHumanMoveSFX() {
        sfxAudioSource.PlayOneShot(humanMoveSFX);
    }

    public void PlaySkeleMoveSFX() {
        sfxAudioSource.PlayOneShot(skeleMoveSFX);
    }

    // Unit Attack Select
    public void PlayMeleeAttackSelectSFX() {
        sfxAudioSource.PlayOneShot(meleeAttackSelectSFX);
    }

    public void PlayRangedAttackSelectSFX() {
        sfxAudioSource.PlayOneShot(rangedAttackSelectSFX);
    }

    // Unit Attack
    public void PlayMeleeAttackSFX() {
        sfxAudioSource.PlayOneShot(meleeAttackSFX);
    }

    public void PlayRangedAttackSFX() {
        sfxAudioSource.PlayOneShot(rangedAttackSFX);
    }

    // Human Unit Death
    public void PlayHumanGigaMungusDeathSFX() {
        sfxAudioSource.PlayOneShot(humanGigaMungusDeathSFX);
    }

    public void PlayHumanSoldierDeathSFX() {
        sfxAudioSource.PlayOneShot(humanSoldierDeathSFX);
    }

    public void PlayHumanArcherDeathSFX() {
        sfxAudioSource.PlayOneShot(humanArcherDeathSFX);
    }

    public void PlayHumanBaldArcherDeathSFX() {
        sfxAudioSource.PlayOneShot(humanBaldArcherDeathSFX);
    }

    // Skele Unit Death
    public void PlaySkeleGigaMungusDeathSFX() {
        sfxAudioSource.PlayOneShot(skeleGigaMungusDeathSFX);
    }

    public void PlaySkeleSoldierDeathSFX() {
        sfxAudioSource.PlayOneShot(skeleSoldierDeathSFX);
    }

    public void PlaySkeleArcherDeathSFX() {
        sfxAudioSource.PlayOneShot(skeleArcherDeathSFX);
    }

    public void PlaySkeleBaldArcherDeathSFX() {
        sfxAudioSource.PlayOneShot(skeleBaldArcherDeathSFX);
    }

    // Other SFX
    public void PlayCancelMoveSFX() {
        sfxAudioSource.PlayOneShot(cancelMoveSFX);
    }

    // Condition SFX
    public void PlayGameWinSFX() {
        sfxAudioSource.PlayOneShot(gameWinSFX);
    }

    public void PlayGameLoseSFX() {
        sfxAudioSource.PlayOneShot(gameLoseSFX);
    }

    // UI SFX
    public void PlayButtonHoverSFX() {
        sfxAudioSource.PlayOneShot(buttonHoverSFX);
    }

    public void PlayButtonClickSFX() {
        sfxAudioSource.PlayOneShot(buttonClickSFX);
    }
}

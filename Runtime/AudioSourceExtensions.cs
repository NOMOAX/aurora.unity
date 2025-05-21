using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="AudioSource"/> 类提供扩展方法。
    /// </summary>
    public static class AudioSourceExtensions
    {
        /// <summary>
        /// 获取当前 <see cref="AudioSource"/> 的状态。
        /// </summary>
        /// <param name="audioSource">音频源。</param>
        /// <returns>音频源的状态。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="audioSource"/> 为 <see langword="null"/>。</exception>
        public static AudioSourceStatus GetStatus(this AudioSource audioSource)
        {
            if (!audioSource)
            {
                throw new ArgumentNullException(nameof(audioSource));
            }
            if (!audioSource.clip)
            {
                return AudioSourceStatus.None;
            }
            if (audioSource.isPlaying)
            {
                return AudioSourceStatus.Playing;
            }
            audioSource.UnPause();
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                return AudioSourceStatus.Paused;
            }
            return AudioSourceStatus.Stopped;
        }

        /// <summary>
        /// 获取当前 <see cref="AudioSource"/> 的进度。
        /// </summary>
        /// <param name="audioSource">音频源。</param>
        /// <returns>音频源的进度。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="audioSource"/> 为 <see langword="null"/>。</exception>
        public static double GetProgress(this AudioSource audioSource)
        {
            if (!audioSource)
            {
                throw new ArgumentNullException(nameof(audioSource));
            }
            var audioClip = audioSource.clip;
            if (!audioClip)
            {
                return 0;
            }
            var samples = audioClip.samples;
            if (samples < 2)
            {
                return 1;
            }
            var timeSamples = audioSource.timeSamples;
            return (double) timeSamples / (samples - 1);
        }

        /// <summary>
        /// 设置当前 <see cref="AudioSource"/> 的进度。
        /// </summary>
        /// <param name="audioSource">音频源。</param>
        /// <param name="progress">进度。</param>
        /// <exception cref="ArgumentNullException"><paramref name="audioSource"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="progress"/> 不在 [0, 1] 范围内。</exception>
        public static void SetProgress(this AudioSource audioSource, double progress)
        {
            if (!audioSource)
            {
                throw new ArgumentNullException(nameof(audioSource));
            }
            if (progress is < 0 or > 1 or float.NaN)
            {
                throw new ArgumentOutOfRangeException(nameof(progress), progress, null);
            }
            var audioClip = audioSource.clip;
            if (!audioClip)
            {
                return;
            }
            var samples = audioClip.samples;
            if (samples < 2)
            {
                return;
            }
            audioSource.timeSamples = (int) ((samples - 1) * progress);
        }
    }
}

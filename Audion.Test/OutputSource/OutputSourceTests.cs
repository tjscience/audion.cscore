using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Audion.Test.OutputSource
{
    [TestClass]
    public class OutputSourceTests
    {
        #region Load

        [TestMethod]
        public void LoadTest()
        {
            var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
            source.Dispose();
        }

        [TestMethod]
        public void LoadVideoTest()
        {
            var source = new Audion.OutputSource(Utils.VideoSampleMP4);
            source.Dispose();
        }

        [TestMethod]
        public void LoadUriTest()
        {
            var source = new Audion.OutputSource(Utils.UriStreamMP3);
            source.Dispose();
        }

        #endregion

        #region Play

        // Plays the audio/video 3 seconds in

        [TestMethod]
        public void PlayTest()
        {
            var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
            source.Play();
            Assert.AreEqual(CSCore.SoundOut.PlaybackState.Playing, source.PlaybackState);
            Utils.Wait(3);
            source.Dispose();
        }

        [TestMethod]
        public void PlayVideoTest()
        {
            var source = new Audion.OutputSource(Utils.VideoSampleMP4);
            source.Play();
            Assert.AreEqual(CSCore.SoundOut.PlaybackState.Playing, source.PlaybackState);
            Utils.Wait(3);
            source.Dispose();
        }

        [TestMethod]
        public void PlayUriTest()
        {
            var source = new Audion.OutputSource(Utils.UriStreamMP3);
            source.Play();
            Assert.AreEqual(CSCore.SoundOut.PlaybackState.Playing, source.PlaybackState);
            Utils.Wait(3);
            source.Dispose();
        }

        #endregion

        #region Stop

        // Plays the audio/video for 1 second, then stops it

        [TestMethod]
        public void StopTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
                source.Play();
                Utils.Wait(1);
                source.Stop();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Stopped, source.PlaybackState);
                source.Dispose();

            }).Wait();
        }

        [TestMethod]
        public void StopVideoTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.VideoSampleMP4);
                source.Play();
                Utils.Wait(1);
                source.Stop();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Stopped, source.PlaybackState);
                source.Dispose();
            }).Wait();
        }

        [TestMethod]
        public void StopUriTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.UriStreamMP3);
                source.Play();
                Utils.Wait(1);
                source.Stop();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Stopped, source.PlaybackState);
                source.Dispose();
            }).Wait();
        }

        #endregion

        #region Pause

        // Plays the audio/video for 1 second, then pauses it

        [TestMethod]
        public void PauseTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
                source.Play();
                Utils.Wait(1);
                source.Pause();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Paused, source.PlaybackState);
                source.Dispose();

            }).Wait();
        }

        [TestMethod]
        public void PauseVideoTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.VideoSampleMP4);
                source.Play();
                Utils.Wait(1);
                source.Pause();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Paused, source.PlaybackState);
                source.Dispose();
            }).Wait();
        }

        [TestMethod]
        public void PauseUriTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.UriStreamMP3);
                source.Play();
                Utils.Wait(1);
                source.Pause();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Paused, source.PlaybackState);
                source.Dispose();
            }).Wait();
        }

        #endregion

        #region Resume

        // Plays the audio/video for 1 second, then pauses it for 1 second, then resumes it and plays for 1 more second.

        [TestMethod]
        public void ResumeTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
                source.Play();
                Utils.Wait(1);
                source.Pause();
                Utils.Wait(1);
                source.Resume();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Playing, source.PlaybackState);
                Utils.Wait(1);
                source.Dispose();

            }).Wait();
        }

        [TestMethod]
        public void ResumeVideoTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.VideoSampleMP4);
                source.Play();
                Utils.Wait(1);
                source.Pause();
                Utils.Wait(1);
                source.Resume();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Playing, source.PlaybackState);
                Utils.Wait(1);
                source.Dispose();
            }).Wait();
        }

        [TestMethod]
        public void ResumeUriTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.UriStreamMP3);
                source.Play();
                Utils.Wait(1);
                source.Pause();
                Utils.Wait(1);
                source.Resume();
                Assert.AreEqual(CSCore.SoundOut.PlaybackState.Playing, source.PlaybackState);
                Utils.Wait(1);
                source.Dispose();
            }).Wait();
        }

        #endregion

        #region Fft Data

        [TestMethod]
        public void GetFftDataTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
                source.FftSize = CSCore.DSP.FftSize.Fft4096;
                source.SourcePropertyChangedEvent += (s, e) =>
                {
                    if (e.Property == Audion.SourceProperty.FftData)
                    {
                        Assert.IsInstanceOfType(e.Value, typeof(float[]));
                        var fftData = (float[])e.Value;
                        Assert.AreEqual((int)source.FftSize, fftData.Length);
                    }
                };

                source.Play();
                Utils.Wait(10);
                source.Stop();
                source.Dispose();

            }).Wait();
        }

        #endregion

        [TestMethod]
        public void ChangeVolumeTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
                source.Volume = 0F;
                source.Play();
                Utils.Wait(1);
                Assert.AreEqual(0F, source.Volume);
                source.Volume = .5F;
                Utils.Wait(1);
                Assert.AreEqual(.5F, source.Volume);
                source.Volume = 1F;
                Utils.Wait(1);
                Assert.AreEqual(1F, source.Volume);
                source.Dispose();

            }).Wait();
        }

        [TestMethod]
        public void PositionTest()
        {
            Task.Factory.StartNew(() =>
            {
                var source = new Audion.OutputSource(Utils.WhiteNoiseSampleMP3);
                source.Position = TimeSpan.FromSeconds(5);
                Assert.AreEqual(5, source.Position.Seconds);
                source.Play();
                Utils.Wait(5);
                Assert.AreEqual(10, source.Position.Seconds);
                source.Stop();
                source.Position = TimeSpan.Zero;
                Assert.AreEqual(0, source.Position.Seconds);
                source.Dispose();

            }).Wait();
        }
    }
}

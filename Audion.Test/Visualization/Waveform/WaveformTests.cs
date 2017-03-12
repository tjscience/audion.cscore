using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Audion.Test.Visualization.Waveform
{
    [TestClass]
    public class WaveformTests
    {
        [TestMethod]
        public void GenerateWaveformTest()
        {
            var source = new Audion.OutputSource();
            var waveform = new Audion.Visualization.Waveform();
            waveform.Source = source;
            source.Load(Utils.WhiteNoiseSampleMP3);
            Utils.Wait(5);
            source.Dispose();
        }
    }
}

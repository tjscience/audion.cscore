using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Audion.Test.UtilsTests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void WhiteNoiseSampleMP3Test()
        {
            var path = Utils.WhiteNoiseSampleMP3;
            Assert.IsTrue(File.Exists(path), "does not exist: " + path);
        }

        [TestMethod]
        public void VideoSampleMP4Test()
        {
            var path = Utils.VideoSampleMP4;
            Assert.IsTrue(File.Exists(path), "does not exist: " + path);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Audion.Test.InputSource
{
    [TestClass]
    public class InputSourceTests
    {
        [TestMethod]
        public void LoadTest()
        {
            var device = Device.GetDefaultRecordingDevice();
            var input = new Audion.InputSource(device);
            input.Dispose();
        }

        [TestMethod]
        public void RecordTest()
        {
            var device = Device.GetDefaultRecordingDevice();
            var input = new Audion.InputSource(device);
            input.Record("test.wav");
            Utils.Wait(10);
            input.Stop();
            input.Dispose();

            // load file as an output source to get the length
            var output = new Audion.OutputSource("test.wav");
            var actual = Math.Round(output.Length.TotalSeconds);
            Assert.AreEqual(10, actual);
            output.Dispose();
        }
    }
}

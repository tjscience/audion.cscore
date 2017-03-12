using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audion.Test
{
    public static class Utils
    {
        private static string _whiteNoiseSampleMP3;
        private static string _videoSampleMP4;
        private static Uri _uriStreamMP3;

        static Utils()
        {
            var root = AppDomain.CurrentDomain.BaseDirectory;
            _whiteNoiseSampleMP3 = System.IO.Path.Combine(root, "Samples", "3185.mp3");
            _videoSampleMP4 = System.IO.Path.Combine(root, "Samples", "SampleVideo_1280x720_30mb.mp4");
            _uriStreamMP3 = new Uri("http://www.siop.org/conferences/09con/17Leadership.mp3");
        }

        public static string WhiteNoiseSampleMP3
        {
            get { return _whiteNoiseSampleMP3; }
        }

        public static string VideoSampleMP4
        {
            get { return _videoSampleMP4; }
        }

        public static Uri UriStreamMP3
        {
            get { return _uriStreamMP3; }
        }

        public static void Wait(int seconds)
        {
            Task.Factory.StartNew(() =>
            {
                System.Threading.Thread.Sleep(seconds * 1000);
            }).Wait();
        }
    }
}

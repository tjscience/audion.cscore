using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Audion
{
    internal static class Application
    {
        static bool framerateSet = false;

        internal static void SetFramerate()
        {
            if (!framerateSet)
            {
                System.Windows.Media.Animation.Timeline.DesiredFrameRateProperty.OverrideMetadata(
                    typeof(System.Windows.Media.Animation.Timeline),
                    new FrameworkPropertyMetadata { DefaultValue = 10 });
                framerateSet = true;
            }
        }
    }
}

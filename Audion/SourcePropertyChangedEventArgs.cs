using System;

namespace Audion
{
    public class SourcePropertyChangedEventArgs : EventArgs
    {
        public SourceProperty Property { get; set; }
        public object Value { get; set; }
    }
}

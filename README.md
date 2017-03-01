# audion
An audio visualization and processing framework for WPF

[![Github-Release](https://img.shields.io/github/release/audion/rubidium.svg)](https://github.com/tjscience/audion/releases)
[![Appveyor-Build](https://ci.appveyor.com/api/projects/status/github/tjscience/audion?svg=true)](https://ci.appveyor.com/project/tjscience/audion)

[![Donate](https://pledgie.com/campaigns/33441.png?skin_name=chrome)](https://pledgie.com/campaigns/33441)

Audion is powered by the awesome **[CSCore](https://github.com/filoe/cscore)** .NET Audio Library

### Included in Audion (so far):

* Waveform
* Timeline
* Spectrum Analyzer
* Intuitive wrapper for managing an audio/video file

### Future Road Map

* Equalizer
* Dynamic Waveform
* Time Clock (Counters)
* and more tba...

### Source

The Source classes in audion makes it simple to load media and start playing with it. Every visualization simply subscribes to a Source. There are two versions, OutputSource to play media out of a device and InputSource to record media from a device. It is common to have one Source instance powering multiple different visualizations. Here is a simple example of loading a media file and playing it.

```C#
var output = new OutputSource();
    
// Open a file dialog to choose a media file
OpenFileDialog openFileDialog = new OpenFileDialog
{
    Filter = Audion.Source.SupportedFiles
};
        
// Load the media file into the source
if (openFileDialog.ShowDialog() == true)
{
    output.Load(openFileDialog.FileName);
}
    
output.Play();
```    
    
### Waveform

![waveform](https://cloud.githubusercontent.com/assets/3706870/22753981/f62a8342-ee0b-11e6-8a75-adb744b969ed.png)

#### XAML

```XML
<audion:Waveform Name="waveform" 
                 Resolution="2048"
                 LeftBrush="LightBlue"                     
                 LeftStroke="Transparent"
                 LeftStrokeThickness="0"
                 CenterLineBrush="White"/>
```

#### Code

```C#
var output = new OutputSource();
waveform.Source = output;
```

### Timeline

![timeline](https://cloud.githubusercontent.com/assets/3706870/22754270/03c1ad68-ee0d-11e6-80bd-edeb34a8ab9e.png)

#### XAML

```XML
<audion:Timeline Name="timeline" 
                 FontSize="12" 
                 ProgressBrush="#9900C3FF"
                 Position="{Binding TrackPosition}"
                 TickBrush="Silver"
                 TimeBrush="Silver"
                 ProgressLineBrush="White"
                 ProgressBrush="#772200EE"/>
```

#### Code

```C#
var output = new OutputSouce();
timeline.Source = output;
```
    
### Spectrum Analyzer

![spectrum](https://cloud.githubusercontent.com/assets/3706870/22754485/abd235fe-ee0d-11e6-848f-c4b5500c9c52.png)

#### XAML

```XML
<audion:SpectrumAnalyzer Name="spectrum" 
                         SpectrumScalingStrategy="Decibel"
                         FrequencyBarCount="100" 
                         FrequencyBarSpacing="2" 
                         FrequencyBarBrush="{StaticResource BarBrush}" />
```

#### Code

```C#
var output = new OutputSource();
spectrum.Source = output;
```

    

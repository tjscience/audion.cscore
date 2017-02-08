# Audion
An audio visualization and processing framework for WPF

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

The Source class in audion makes it simple to load media and start playing with it. Every visualization simply subscribes to a Source. It is common to have one Source instance powering multiple different visualizations. Here is a simple example of loading a media file and playing it.

    var source = new Source();
    
    // Open a file dialog to choose a media file
    OpenFileDialog openFileDialog = new OpenFileDialog
    {
        Filter = Audion.Source.SupportedFiles
    };
        
    // Load the media file into the source
    if (openFileDialog.ShowDialog() == true)
    {
        source.Load(openFileDialog.FileName);
    }
    
    source.Play();
    
### Waveform

![waveform](https://cloud.githubusercontent.com/assets/3706870/22753981/f62a8342-ee0b-11e6-8a75-adb744b969ed.png)

#### XAML

    <audion:Waveform Name="waveform" 
                     Resolution="2048"
                     LeftBrush="LightBlue"                     
                     LeftStroke="Transparent"
                     LeftStrokeThickness="0"
                     CenterLineBrush="White"/>

#### Code

    var _source = new Source();
    waveform.Source = _source;

### Timeline

![timeline](https://cloud.githubusercontent.com/assets/3706870/22754270/03c1ad68-ee0d-11e6-80bd-edeb34a8ab9e.png)

#### XAML

    <audion:Timeline Name="timeline" 
                     FontSize="12" 
                     ProgressBrush="#9900C3FF"
                     Position="{Binding TrackPosition}"
                     TickBrush="Silver"
                     TimeBrush="Silver"
                     ProgressLineBrush="White"
                     ProgressBrush="#772200EE"/>

#### Code

    var _source = new Source();
    timeline.Source = _source;
    
### Spectrum Analyzer

![spectrum](https://cloud.githubusercontent.com/assets/3706870/22754485/abd235fe-ee0d-11e6-848f-c4b5500c9c52.png)

#### XAML

    <audion:SpectrumAnalyzer Name="spectrum" 
                             SpectrumScalingStrategy="Decibel"
                             FrequencyBarCount="100" 
                             FrequencyBarSpacing="2" 
                             FrequencyBarBrush="{StaticResource BarBrush}" />

#### Code

    var _source = new Source();
    spectrum.Source = _source;
    

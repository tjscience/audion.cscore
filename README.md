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

### Waveform

![waveform](https://cloud.githubusercontent.com/assets/3706870/22753981/f62a8342-ee0b-11e6-8a75-adb744b969ed.png)

#### XAML

    <audion:Waveform Name="waveform" Resolution="2048" />

#### Code

    var _source = new Source();
    waveform.Source = _source;

### Timeline

![timeline](https://cloud.githubusercontent.com/assets/3706870/22754270/03c1ad68-ee0d-11e6-80bd-edeb34a8ab9e.png)

#### XAML

    <audion:Timeline Name="timeline" FontSize="12" ProgressBrush="#9900C3FF" />

#### Code

    var _source = new Source();
    timeline.Source = _source;
    
### Spectrum Analyzer

![spectrum](https://cloud.githubusercontent.com/assets/3706870/22754485/abd235fe-ee0d-11e6-848f-c4b5500c9c52.png)

#### XAML

    <audion:SpectrumAnalyzer Name="spectrum" SpectrumScalingStrategy="Decibel"
                             FrequencyBarCount="100" FrequencyBarSpacing="2" FrequencyBarBrush="{StaticResource BarBrush}" />

#### Code

    var _source = new Source();
    spectrum.Source = _source;
    

# Download VolcaTool
[Download a zip file of the latest version.](https://dl.dropboxusercontent.com/u/14130241/VolcaTool-1.1.zip)

# About VolcaTool
VolcaTool is an open source windows utility to port folders of wav files to the Korg Volca Sample digital sample sequencer. It is written in C# using the Syro API.

This is a preview version. This software is not written or endorsed by Korg. Use at your own risk.

# Quick Start:
1. Arrange the samples you'd like on your VolcaSample into a directory.
2. VolcaSampleCmdLineTool.exe -dir=(SAMPLE_DIR)
3. Play the resulting Result.wav file with your VolcaSample connected to your computer via the sync-in port.

# Command line options:
--help                 
Show this message and exit.

--dir=VALUE           
[REQUIRED] Directory containing samples.

--result=VALUE         
Provide a filename to create the resulting wav file (default is 'result.wav').

--sns, --sample_number_start=VALUE
The sample slot number to start writing to. This is the starting sample slot number, samples are added incrementally after it.

--max_seconds=VALUE    
The longest a sample can be (in seconds) before it is trimmed.

# About Syncing:

> Connect the output of your playback device with a stereo cable to the SYNC IN port of your volca sample. Turn the volume up.
>
> Now playback the generated syrostream and the volca sample will enter receive mode.
>
> Be careful when playing syro stream through speakers or headphones. Playback at large volumes may cause damage to equipment and/or ears.

If you have errors syncing, the most likely culprit is your PC’s audio setup. Be sure to disable any sort of EQ or effects. If you have beats audio, you will probably have to uninstall it. Don’t forget about Dre.

# Download VolcaTool
[Download a zip file of the latest version.](https://dl.dropboxusercontent.com/u/14130241/volcatool-1.3.zip)


# About VolcaTool
VolcaTool is an open source windows utility to port folders of wav files to the Korg Volca Sample digital sample sequencer. It is written in C# using the Syro API.

This software is not written or endorsed by Korg. Use at your own risk.

# Usages:

*Transfer a directory of files:*
volcatool -mode=transfer -dir=.\samples

*Fill slots starting at a slot number:*
volcatool -mode=transfer -dir=.\samples --sample_number_start=10

*Erase all sample slots:*
volcatool -mode=erase -erase=0-99

*Erase a range of sample slots:*
volcatool -mode=erase -erase=25-30

*Erase a list of sample slots:*
volcatool -mode=erase -erase=0,1,2,3,4

*Erase a single sample slot:* 
volcatool -mode=erase -erase=42

# Command line options:
      --help                 Show this message and exit.
      --mode=VALUE           [REQUIRED] can be either 'erase' or 'transfer'.
      --erase=VALUE          [REQUIRED for 'erase' mode.] Slots to erase in
                               format of: A range (ex: 0-99), or a comma
                               seperated list (ex: 2,6,25,42), or a single slot
                               number.
      --dir=VALUE            [REQUIRED for 'transfer' mode.] Directory
                               containing samples to transfer. REQUIRED if
                               transferring.
      --result=VALUE         Provide a filename to create the resulting wav
                               file (default is 'result.wav').
      --sns, --sample_number_start=VALUE
                             The sample slot number to start writing to. This
                               is the starting sample slot number, samples are
                               added incrementally after it.
      --max_seconds=VALUE    The longest a sample can be (in seconds) before
                               it is trimmed.


# About Syncing:

> Connect the output of your playback device with a stereo cable to the SYNC IN port of your volca sample. Turn the volume up.
>
> Now playback the generated syrostream and the volca sample will enter receive mode.
>
> Be careful when playing syro stream through speakers or headphones. Playback at large volumes may cause damage to equipment and/or ears.

If you have errors syncing, the most likely culprit is your PC’s audio setup. Be sure to disable any sort of EQ or effects. If you have beats audio, you will probably have to uninstall it. Don’t forget about Dre.

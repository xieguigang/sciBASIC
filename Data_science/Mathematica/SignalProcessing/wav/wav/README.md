# WAV Audio File Reader, Writer and Voiceprint Extractor

Reads and writes RIFF/WAVE audio files and extracts fixed-dimension mel-filterbank voiceprint vectors, as part of sciBASIC#.

## Overview
- Parses the RIFF/WAVE container: `RIFF`/`WAVE` magic, the `fmt ` sub-chunk (format, channels, sample rate, byte rate, block align, bit depth, WAVE_FORMAT_EXTENSIBLE sub-format) and the `data` sub-chunk.
- Decodes 8/16/24/32/64-bit PCM, 32/64-bit IEEE float and G.711 A-law/μ-law frames into per-channel samples normalized to [-1.0, 1.0], either eagerly or lazily streamed.
- Writes WAV files from normalized sample data for PCM, IEEE float and G.711 encodings at 8/16/24/32/64 bits per sample.
- Pure-DSP voiceprint pipeline: pre-emphasis, framing, Hamming window, FFT, mel filterbank, DCT, delta and delta-delta coefficients, CMVN and L2 normalisation.

## Key Types
- `Microsoft.VisualBasic.Data.Wave.WaveFile` — the WAV file model; `Open` reads the header, `fmt` and `data` sub-chunks, with an optional lazy streaming mode.
- `Microsoft.VisualBasic.Data.Wave.FMTSubChunk` — the `fmt ` sub-chunk: audio format, channel count, sample rate, block align, bit depth and extensible-format fields.
- `Microsoft.VisualBasic.Data.Wave.DataSubChunk` / `LazyDataChunk` — eager and streaming `data` sub-chunk implementations exposing `LoadSamples`.
- `Microsoft.VisualBasic.Data.Wave.Sample` — one sample frame holding per-channel values; `left` and `right` give the stereo channels.
- `Microsoft.VisualBasic.Data.Wave.WaveWriter` — static writer producing PCM, IEEE float and A-law/μ-law WAV files from normalized samples.
- `Microsoft.VisualBasic.Data.Wave.VoicePrintExtractor` — the voiceprint pipeline (`Extract`, `ExtractDetailed`) working on a normalized sample array.
- `Microsoft.VisualBasic.Data.Wave.VoicePrintOptions` — extraction parameters: target dimension, mel filter count, frame size/hop, delta flags, CMVN and L2 normalisation.
- `Microsoft.VisualBasic.Data.Wave.WavVoicePrintReader` — one-call extraction from a WAV path with channel selection, time-window cropping and `GetWavInfo`.

## Quick Start
```vbnet
Imports System.IO
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.Wave

' open a RIFF/WAVE file (lazy: samples are decoded on demand)
Using reader As New BinaryDataReader(File.OpenRead("audio.wav"))
    Using wav As WaveFile = WaveFile.Open(reader, lazy:=True)
        Console.WriteLine($"{wav.fmt.SampleRate} Hz, {wav.fmt.channels} ch, {wav.fmt.BitsPerSample} bit")

        Dim frames As Sample() = wav.data.LoadSamples(0, 4096).ToArray()
        Console.WriteLine(frames(0).left)
    End Using
End Using

' extract a mel-filterbank voiceprint vector for the first 30 seconds
Dim result As VoicePrintResult = VoicePrint.Extract("audio.wav", startTime:=0, endTime:=30, targetDim:=192)
Dim vector As Double() = result.Vector
```

## Package
- Assembly: `Microsoft.VisualBasic.Data.Wave`
- TargetFramework: `net10.0`
- Tags: `scibasic;wav;audio;voiceprint;mfcc;riff`

## License
GPL-3.0-or-later

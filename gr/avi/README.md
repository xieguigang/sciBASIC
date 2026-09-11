# AVI Video Encoder For Uncompressed RGB Frame Streams

A small, dependency-free AVI (RIFF) writer that turns a stream of uncompressed RGB frames into a playable `.avi` file.

## Overview
- Assembles the RIFF/AVI container by hand: the `RIFF`/`AVI ` header, the `hdrl` list, per-stream `strl`/`strh`/`strf`/`indx` chunks and the `movi` data list.
- Frames are accepted as `System.Drawing.Bitmap`, as a `Color()` pixel row array, or as a flat RGBA byte array; each frame is buffered to a temp file, so long sequences do not have to be held in memory.
- Streams are declared with `fps`, `width` and `height` and appended to before the file is finalized by a single `WriteBuffer` call.
- All bytes go through a resizable `UInt8Array` buffer that writes directly into the target file path.

## Key Types
- `Microsoft.VisualBasic.Imaging.AVIMedia.Encoder` — top-level container writer; owns the settings, the main header and the stream list, and emits the final file.
- `Microsoft.VisualBasic.Imaging.AVIMedia.Settings` — the video canvas size (`width`/`height`) shared by all streams.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AVIStream` — one video track (fps/size) with its `FrameStream` list and `addFrame` / `addRGBFrame` methods.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AVIMainHeader` — the `avih` chunk: frame rate, total frame count, stream count and suggested buffer size.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AVIStreamHeader` — the `strh` chunk (`vids` / `DIB ` fourCC, scale/rate, sample size) plus the `StreamTypes` enum.
- `Microsoft.VisualBasic.Imaging.AVIMedia.FrameStream` — a single frame payload with its byte length.
- `Microsoft.VisualBasic.Imaging.AVIMedia.UInt8Array` — low-level resizable byte buffer with `writeInt`, `writeLong`, `writeString` and `subarray`.

## Quick Start
```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.AVIMedia

Dim encoder As New Encoder(New Settings With {.width = 800, .height = 600})
Dim video As New AVIStream(25, 800, 600)

For Each frame As Bitmap In renderedFrames
    Call video.addFrame(frame)
Next

encoder.streams.Add(video)
Call encoder.WriteBuffer("animation.avi")
```

## Package
- Assembly: `Microsoft.VisualBasic.Imaging.AVIMedia`
- TargetFramework: `net10.0`
- Tags: `scibasic;avi;video-encoder;riff;animation`

## License
GPL-3.0-or-later

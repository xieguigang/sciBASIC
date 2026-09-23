# AVI Video Encoder With Per-Stream Video Codecs

A dependency-free AVI (RIFF) writer that turns a stream of rendered frames into a playable `.avi` file, with a pluggable video codec layer for each video stream.

## Overview

- Assembles the RIFF/AVI container by hand: the `RIFF`/`AVI ` header, the `hdrl` list, per-stream `strl`/`strh`/`strf`/`indx` chunks and the `movi` data list.
- Frames are accepted as a `Bitmap`, as a `Color()` pixel row array, or as a flat RGBA byte array; each frame is buffered to a temp file, so long sequences do not have to be held in memory.
- Every video stream owns its own `AviVideoCodec` instance, so different streams inside one file may use different compression methods.
- `strh.fccHandler`, `strf.biCompression`, `biBitCount`, `biHeight` sign and the suggested buffer size are all driven by the codec descriptor instead of being hard coded.
- Chunk padding is applied consistently: odd sized frame payloads are padded to the RIFF word boundary, and the `indx` offsets account for that padding, so compressed frames (whose sizes are usually odd) stay indexable.

## Video Codecs

| `AviCodec` | fourCC | Description |
| --- | --- | --- |
| `MJPEG` *(default)* | `MJPG` | Every frame is a standard JPEG image, encoded with the GDI+ JPEG encoder. Frame independent, so any frame can be decoded on its own. The JPEG quality is configurable (`0` - `100`, default `90`). |
| `DIB` | `DIB ` | Uncompressed 32bpp top-down bitmap, i.e. the very same output as the pre-codec versions of this library. Use it to keep the previous behaviour (much larger files). |
| `DivX` / `XviD` | `DIVX` / `XVID` | MPEG-4 Part 2 (ASP). **Not available yet**: selecting it throws a `NotSupportedException`, see "Known Limitations". |

## Key Types

- `Microsoft.VisualBasic.Imaging.AVIMedia.Encoder` — top-level container writer; owns the settings, the main header and the stream list, and emits the final file.
- `Microsoft.VisualBasic.Imaging.AVIMedia.Settings` — the video canvas size (`width`/`height`) shared by all streams.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AVIStream` — one video track (fps/size/codec) with its `FrameStream` list and `addFrame` / `addRGBFrame` methods.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AviCodec` — the compression method selected per stream.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AviVideoCodec` — the codec contract (`FourCC`, `BitCount`, `HeightSign`, `SuggestedBufferSize`, `CodecPrivate`, `Encode`).
- `Microsoft.VisualBasic.Imaging.AVIMedia.DibCodec` / `MjpegCodec` — the built-in implementations.
- `Microsoft.VisualBasic.Imaging.AVIMedia.PixelData` — bitmap/pixel buffer conversions shared by the codecs.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AVIMainHeader` — the `avih` chunk: frame interval derived from the stream fps, total frame count, stream count and suggested buffer size.
- `Microsoft.VisualBasic.Imaging.AVIMedia.AVIStreamHeader` — the `strh` chunk model plus the `StreamTypes` enum.
- `Microsoft.VisualBasic.Imaging.AVIMedia.FrameStream` — a single frame payload with its byte length and padded chunk size.
- `Microsoft.VisualBasic.Imaging.AVIMedia.UInt8Array` — low-level resizable byte buffer with `writeInt`, `writeLong`, `writeString` and `subarray`.

## Quick Start

Default (MJPEG) output:

```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.AVIMedia

Dim encoder As New Encoder(New Settings With {.width = 800, .height = 600})
Dim video As New AVIStream(25, 800, 600)   ' AviCodec.MJPEG is the default

For Each frame As Bitmap In renderedFrames
    Call video.addFrame(frame)
Next

encoder.streams.Add(video)
Call encoder.WriteBuffer("animation.avi")
```

MJPEG with an explicit JPEG quality:

```vbnet
Dim video As New AVIStream(25, 800, 600, AviCodec.MJPEG, quality:=75)
```

Uncompressed output, i.e. the pre-codec behaviour:

```vbnet
Dim video As New AVIStream(25, 800, 600, AviCodec.DIB)
```

Different codecs for different streams:

```vbnet
Dim encoder As New Encoder(New Settings With {.width = 800, .height = 600})
Call encoder.streams.Add(New AVIStream(25, 800, 600, AviCodec.MJPEG, quality:=90))
Call encoder.streams.Add(New AVIStream(25, 800, 600, AviCodec.DIB))
Call encoder.WriteBuffer("mixed.avi")
```

Frames may also be pushed as raw pixels instead of a bitmap:

```vbnet
Call video.addFrame(pixels)      ' pixels As Color(), row major
Call video.addRGBFrame(rgba)     ' rgba As Byte(), flat (r, g, b, a) quadruples
```

## Known Limitations

- **DivX/XviD (`AviCodec.DivX` / `AviCodec.XviD`) is not implemented.** Building a real MPEG-4 Part 2 (ASP) encoder requires the ISO/IEC 14496-2 macroblock layer VLC tables (MCBPC, CBPY, intra DC size, TCOEFF and motion vector tables). Those tables are not part of this repository, and writing them down from memory cannot be validated, so selecting these codecs throws a `NotSupportedException` instead of producing a file that no player can decode.
- `AviCodec.MJPEG` needs GDI+ for the JPEG encoding step, therefore it is Windows only. The uncompressed `AviCodec.DIB` path is pure managed.
- The frame size must match the canvas size a video stream was created with; a mismatch throws an `ArgumentException` instead of writing a broken file.
- Blast radius of the default change: `New AVIStream(fps, width, height)` now produces MJPEG instead of uncompressed frames. Pass `AviCodec.DIB` explicitly to restore the previous output.

## Package

- Assembly: `Microsoft.VisualBasic.Imaging.AVIMedia`
- TargetFramework: `net10.0`
- Tags: `scibasic;avi;video-encoder;riff;animation;mjpeg`

## License

GPL-3.0-or-later

# GDI+ Graphics Wrapper And System.Drawing Interop For Windows

A Windows-only GDI+ layer that gives sciBASIC# a concrete raster canvas, image filters, file encoders and font metrics on top of `System.Drawing`.

## Overview
- `Graphics2D` wraps `System.Drawing.Graphics` behind the shared `IGraphics`/`GDICanvas` contract used by the rest of the imaging stack: lines, curves, beziers, paths, images and text.
- Interop converters translate between `System.Drawing` types (Color, Font, Brush, Pen, GraphicsPath, Bitmap) and the sciBASIC# imaging/CSS models.
- Image processing helpers: brightness/contrast/gamma adjustment, blur, vignette, drop shadows, rotation, unscaled resizing, avatar trimming and bitonal conversion.
- File encoders for animated GIF (`GifEncoder`) and multi-page TIFF (`TiffWriter`), plus image-format resolution from strings and extensions.
- Text utilities: `FontMetrics` measurement, `GraphicsText` rotated/in-rectangle layout, and `TextRender` for HTML-like markup drawing.

## Key Types
- `Microsoft.VisualBasic.Drawing.Graphics2D` — GDI+ device handle over a bitmap canvas; implements the shared drawing surface and can be saved to a file or stream.
- `Microsoft.VisualBasic.Drawing.Extensions` — factory helpers: `CreateGDIDevice`, `CreateCanvas2D`, `CanvasCreateFromImageFile` and icon conversion.
- `Microsoft.VisualBasic.Drawing.Interop.GDIPlusImage` / `.BitmapBuffer` — bridge between sciBASIC# `Image`/`Bitmap` and `System.Drawing`, plus fast pixel buffers.
- `Microsoft.VisualBasic.Drawing.Drawing2D.Text.FontMetrics` — string measurement that only works on the GDI+ graphics context.
- `Microsoft.VisualBasic.Drawing.Drawing2D.Text.GraphicsText` — draws rotated text and lays out text inside a rectangle.
- `Microsoft.VisualBasic.Drawing.GifEncoder` — streams multiple frames into an animated GIF; disposing the encoder completes the file.
- `Microsoft.VisualBasic.Drawing.TiffWriter` — collects images and saves them as a multi-page TIFF.
- `Microsoft.VisualBasic.Drawing.Imaging.BitmapImage.Effects` / `.Utils` — image filters and resizing/cropping helpers.

## Quick Start
```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing

Using canvas As Graphics2D = New Size(800, 600).CreateGDIDevice(filled:=Color.White)
    Call canvas.DrawString("sciBASIC#", 40, 40)
    Call canvas.DrawLine(40, 90, 760, 90)
    Call canvas.Save("demo.png")
End Using
```

## Package
- Assembly: `Microsoft.VisualBasic.Drawing.Windows`
- TargetFramework: `net10.0-windows`
- Tags: `scibasic;gdi-plus;system-drawing;graphics-interop;windows-forms`

## License
GPL-3.0-or-later

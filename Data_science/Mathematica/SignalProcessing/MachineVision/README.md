# Machine Vision Tools: Shape, Blob, OCR and Tracking

A 2D computer-vision toolbox for sciBASIC# covering curve and shape comparison, blob extraction, circle/square detection, OCR text matching and multi-object tracking.

## Overview
- Curve and shape analysis: Procrustes normalisation and rotation, Frechet distance, subdivision, rebalancing and shape similarity scoring.
- Blob extraction: two-pass connected component labelling that returns one polygon per connected region.
- Detection: Hough and vector-vote circle detection, square detection and edge extraction from bitmap buffers.
- Recognition and tracking: OCR frame scanning with a confusion-character matrix, text matching, RANSAC point-set alignment with FPFH/Shape Context descriptors, Hungarian-assignment based frame tracking and trajectory accumulation.

## Key Types
- `Microsoft.VisualBasic.Math.MachineVision.CCL.CCLabeling` — two-pass connected component labelling over a `BitmapBuffer`.
- `Microsoft.VisualBasic.Math.MachineVision.CurveAnalysis` — Procrustes normalisation, Frechet distance, shape similarity and curve resampling.
- `Microsoft.VisualBasic.Math.MachineVision.Curve` — an ordered point list representing a 2D curve.
- `Microsoft.VisualBasic.Math.MachineVision.HoughCircles.Algorithm` — `CircleHough`, `CircleVector` and `SquareVector` detectors.
- `Microsoft.VisualBasic.Math.MachineVision.OcrFrameScan` — frame-level OCR result set with score/length filtering.
- `Microsoft.VisualBasic.Math.MachineVision.ConfusionChars` — confusion matrix used when matching confusable OCR characters.
- `Microsoft.VisualBasic.Math.MachineVision.RANSACPointAlignment` — RANSAC alignment of two 2D polygons with least-squares refinement.
- `Microsoft.VisualBasic.Math.MachineVision.Tracker` — Hungarian-assignment multi-object tracker producing `Trajectory` objects.

## Quick Start
```vbnet
Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math.MachineVision.CCL
Imports Microsoft.VisualBasic.Math.MachineVision.HoughCircles

Dim buffer As BitmapBuffer = BitmapBuffer.FromImage(Image.FromFile("cells.png"))

' one polygon per connected blob
Console.WriteLine(CCLabeling.TwoPassProcess(buffer).Count)

' circle detection over the same buffer
Dim circles = Algorithm.CircleHough(buffer)
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.MachineVision`
- TargetFramework: `net10.0`
- Tags: `scibasic;machine-vision;shape-matching;hough-transform;connected-component;tracking`

## License
GPL-3.0-or-later

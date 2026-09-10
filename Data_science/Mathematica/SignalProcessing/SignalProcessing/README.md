# Signal Processing: FFT, Wavelets, Filters and Peak Detection

A one-dimensional signal processing toolkit for sciBASIC#, covering transforms, filtering, peak detection, alignment and signal synthesis.

## Overview
- Fourier analysis through `FFT.FourierTransform` (DFT/FFT, 1-D and 2-D, real and complex) and multi-level wavelet transforms with Daubechies, Symlet and Coiflet bases.
- Smoothing and conditioning: Savitzky-Golay filtering, trend removal (de-trend/re-trend), zero elimination, Ramer-Douglas-Peucker decimation and mean/continuous padding.
- Peak detection via the accumulated-elevation algorithm with quantile baseline estimation, plus EM-Gaussian mixture fitting and 1-D/2-D Kalman filtering.
- Signal comparison and alignment: dynamic time warping (DTW) with pluggable preprocessors, correlation optimized warping (COW), resampling and interpolation.
- Composable synthesis from basis functions (sine, damped sine, Gaussian, Lorentzian, Ricker, log-normal, trends, thresholds, noise) and ready-made presets such as ECG, vibration and weather.

## Key Types
- `Microsoft.VisualBasic.Math.SignalProcessing.GeneralSignal` — the core x/y signal tuple (`Measures` vs `Strength`) with reference id, metadata, range slicing and point enumeration.
- `Microsoft.VisualBasic.Math.SignalProcessing.FFT.FourierTransform` — static `DFT`/`FFT` entry points for `Complex()` and 2-D complex buffers, forward or inverse.
- `Microsoft.VisualBasic.Math.SignalProcessing.WaveletTransform.Transform` — multi-level forward/inverse wavelet transform plus detail and scaling coefficient extraction.
- `Microsoft.VisualBasic.Math.SignalProcessing.Filters.SGFilter` — Savitzky-Golay smoothing with coefficient computation, padding and chainable preprocessors.
- `Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding.ElevationAlgorithm` — detects peaks from the accumulated elevation line of a signal.
- `Microsoft.VisualBasic.Math.SignalProcessing.NDtw.Dtw` — dynamic time warping distance between two sequences with selectable distance measure.
- `Microsoft.VisualBasic.Math.SignalProcessing.COW.CowAlignment` — correlation optimized warping and linear alignment of chromatographic traces.
- `Microsoft.VisualBasic.Math.SignalProcessing.Source.SignalGenerator` — builds composite signals from basis functions and samples them onto an arbitrary grid.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Math.SignalProcessing.Filters
Imports Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding
Imports Microsoft.VisualBasic.Math.SignalProcessing.Source

' synthesise: linear trend + sine + a Gaussian peak
Dim gen As New SignalGenerator() _
    .Add(Basis.Linear(amp:=0.01, center:=0, scale:=1)) _
    .Add(Basis.Sine(amp:=1, center:=0, scale:=50)) _
    .Add(Basis.Gaussian(amp:=3, center:=200, sigma:=10))

Dim x As Double() = Enumerable.Range(0, 1000).Select(Function(i) CDbl(i)).ToArray()
Dim signal As New GeneralSignal With {
    .reference = "demo",
    .Measures = x,
    .Strength = gen.Sample(x)
}

' Savitzky-Golay smoothing
Dim sg As New SGFilter(5, 5)
Dim coeffs As Double() = SGFilter.computeSGCoefficients(5, 5, 4)
Dim smoothed As Double() = sg.smooth(signal.Strength, coeffs)

' peak detection
Dim peaks = New ElevationAlgorithm(angle:=30, baselineQuantile:=0.25).FindAllSignalPeaks(signal)
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.SignalProcessing`
- TargetFramework: `net10.0`
- Tags: `scibasic;signal-processing;fft;wavelet;peak-detection;filtering`

## License
GPL-3.0-or-later

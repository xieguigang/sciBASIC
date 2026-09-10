# Random Number Generators and Seeded Samplers Library

Pseudo-random number generation for sciBASIC#, offering reproducible seeded generators, a Mersenne Twister implementation and normal-deviate sampling helpers for Monte Carlo work.

## Overview
- `FastRandom`: a fast xorshift generator (period 2^128-1) with cheap seed re-initialisation, usable as an `IProvideRandomValues`.
- `MersenneTwisterFast`: classic MT19937 with integer, Gaussian, gamma, float, permutation and shuffle samplers.
- `ThreadSafeFastRandom`: thread-safe wrapper around the fast generator.
- `Randomizer`: a `Random` subclass seeded from the RAND random-number tables providing normal deviates, integer/percentage samples and shuffling.
- `crandn`: seeded helpers that return uniform or normally distributed `Vector`/matrix samples for Monte Carlo simulation.

## Key Types
- `Microsoft.VisualBasic.Math.FastRandom` — fast xorshift generator with `Next`, `NextDouble`, `NextFloat`, `NextUInt`, `NextInt`, `NextBool` and `Reinitialise`.
- `Microsoft.VisualBasic.Math.ThreadSafeFastRandom` — thread-safe accessor over the fast generator.
- `Microsoft.VisualBasic.Math.IProvideRandomValues` — abstraction so a generator can be injected and replaced.
- `Microsoft.VisualBasic.Math.MersenneTwisterFast` — Mersenne Twister with `nextDouble`, `nextGaussian`, `nextGamma`, `shuffle` and `permute`.
- `Microsoft.VisualBasic.Math.Randomizer` — `Random` subclass exposing `GetRandomNormalDeviates`, `GetRandomInts`, `GetRandomPercentages`.
- `Microsoft.VisualBasic.Math.crandn` — `randn`/`rand` returning seeded `Vector` or matrix samples.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Dim rng As New FastRandom(seed:=42)
Dim x As Double = rng.NextDouble()

Dim mt As New MersenneTwisterFast()
Dim g As Double = mt.nextGaussian()

' seeded normal deviates for Monte Carlo simulation
Dim sample As Vector = crandn.randn(1000, seed:=42)
```

## Package
- Assembly: `Microsoft.VisualBasic.Math.Randomizer`
- TargetFramework: `net10.0`
- Tags: `scibasic;random;mersenne-twister;monte-carlo;pseudorandom;sampling`

## License
GPL-3.0-or-later

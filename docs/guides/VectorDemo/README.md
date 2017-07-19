# Vector Shadows for VB.NET language

```vbnet
Dim strings = New Func(Of WeightString)(AddressOf WeightString.Rand) _
    .RepeatCalls(2000, sleep:=2) _
    .VectorShadows

Dim asciiRands$() = strings.str
Dim strWeights#() = strings.weight

Dim subsetLessThan50 As WeightString() = strings(strings <= 50)
Dim subsetGreaterThan90 As WeightString() = strings(strings >= 90)

strings.weight = 2000.Sequence.As(Of Double)

subsetLessThan50 = strings(strings <= 50)
subsetGreaterThan90 = strings(strings >= 90)

Dim target As Char = RandomASCIIString(20)(10)

Dim charsCount%() = strings.Count(target)
Dim sums%() = strings.Sum
```

## Advantages

+ Brief than Linq
+ Coding in more elegant way
+ Is an functional programming language feature

## Disadvantage

+ Didn't have well support from VisualStudio IntelliSense but have supports in ``notepad++`` and ``VisualStudio Code`` editor
+ Usually slower than Linq, but sometime even faster than Linq from the demo benchmark result
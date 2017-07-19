# Vector Shadows for VB.NET language

Introduce the Vector Shadows language feature for VisualBasic functional programming. You can shadowing any .NET type as a generic vector object, for example:

```vbnet
Class Foo
    Public Property str As String
    
    Sub New(s$)
        str = s
    End Sub
    
    Public Shared Overloads Operator Like(s As Foo, s$) As Boolean
        Return Regex.Match(s.str, s).Success
    End Operator
End Class
```

A .NET example type ``Foo`` which have a property named ``str`` with ``String`` property type, and have a operator ``Like`` for determine its string value match the regular expression pattern or not.

So that if we have a such ``Foo`` type array, like:

```vbnet
Dim array = {New Foo("123"), New Foo("ABC"), New Foo("!@#")}
```

Then wen can using linq for some functional programming like:

```
Dim isNumeric = array.Select(Function(s) s Like "\d+").ToArray
Dim Allstrings = array.Select(Function(s) s.str).ToArray
```

Using Linq is very brief in collection operation when comparing with the ``For...Next`` loop imperative programming, but we can makes this code more brief using Vector Shadows language feature in VisualBasic:

```vbnet
' dynamics shadowing a vector from any .NET collection type
' Example from the array object that we declared above
Dim strings = {New Foo("123"), New Foo("ABC"), New Foo("!@#")}.VectorShadows
```

And then we can invoke the operator/function get/set property in a more brief way:

```vbnet
Dim isNumeric = strings Like "\d+"
Dim Allstrings = strings.str
```

## Demo

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

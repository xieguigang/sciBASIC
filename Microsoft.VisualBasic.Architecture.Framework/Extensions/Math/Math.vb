Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("VBMath", Publisher:="xie.guigang@gmail.com")>
Public Module VBMathExtensions

    ''' <summary>
    ''' Logical true values are regarded as one, false values as zero. For historical reasons, NULL is accepted and treated as if it were integer(0).
    ''' </summary>
    ''' <param name="bc"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Sum")>
    <Extension> Public Function Sum(bc As Generic.IEnumerable(Of Boolean)) As Double
        If bc.IsNullOrEmpty Then
            Return 0
        End If

        Dim LQuery = (From b In bc Select If(b, 1, 0)).ToArray
        Dim value As Double = LQuery.Sum
        Return value
    End Function

    <ExportAPI("Median")>
    <Extension> Public Function Median(data As Generic.IEnumerable(Of Double)) As Double
        Dim ordered = (From n In data Select n Order By n Ascending).ToArray
        Dim m = data.Count / 2
        Return ordered(m)
    End Function

    ''' <summary>
    ''' Standard Deviation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("STD", Info:="Standard Deviation")>
    <Extension> Public Function STD(values As Generic.IEnumerable(Of Double)) As Double
        Dim Avg As Double = values.Average
        Dim LQuery = (From n In values Select (n - Avg) ^ 2).ToArray
        Return System.Math.Sqrt(LQuery.Sum / values.Count)
    End Function

    ''' <summary>
    ''' Standard Deviation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("STD", Info:="Standard Deviation")>
    <Extension> Public Function STD(values As Generic.IEnumerable(Of Integer)) As Double
        Dim Avg As Double = values.Average
        Dim LQuery = (From n In values Select (n - Avg) ^ 2).ToArray
        Return System.Math.Sqrt(LQuery.Sum / values.Count)
    End Function

    ''' <summary>
    ''' Standard Deviation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("STD", Info:="Standard Deviation")>
    <Extension> Public Function STD(values As Generic.IEnumerable(Of Long)) As Double
        Dim Avg As Double = values.Average
        Dim LQuery = (From n In values Select (n - Avg) ^ 2).ToArray
        Return System.Math.Sqrt(LQuery.Sum / values.Count)
    End Function

    ''' <summary>
    ''' Standard Deviation
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("STD", Info:="Standard Deviation")>
    <Extension> Public Function STD(values As Generic.IEnumerable(Of Single)) As Double
        Dim Avg As Double = values.Average
        Dim LQuery = (From n In values Select (n - Avg) ^ 2).ToArray
        Return System.Math.Sqrt(LQuery.Sum / values.Count)
    End Function

    ''' <summary>
    ''' 多位坐标的欧几里得距离
    ''' </summary>
    ''' <param name="Vector"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Euclidean", Info:="Euclidean Distance")>
    <Extension> Public Function EuclideanDistance(Vector As Generic.IEnumerable(Of Double)) As Double
        Return Math.Sqrt((From n In Vector Select n ^ 2).ToArray.Sum)
    End Function

    <ExportAPI("Euclidean", Info:="Euclidean Distance")>
    <Extension> Public Function EuclideanDistance(Vector As Generic.IEnumerable(Of Integer)) As Double
        Return Math.Sqrt((From n In Vector Select n ^ 2).ToArray.Sum)
    End Function

    <ExportAPI("Euclidean", Info:="Euclidean Distance")>
    <Extension> Public Function EuclideanDistance(a As Generic.IEnumerable(Of Integer), b As Generic.IEnumerable(Of Integer)) As Double
        If a.Count <> b.Count Then
            Return -1
        Else
            Return Math.Sqrt((From i As Integer In a.Sequence.AsParallel Select (a(i) - b(i)) ^ 2).ToArray.Sum)
        End If
    End Function

    <ExportAPI("Euclidean", Info:="Euclidean Distance")>
    <Extension> Public Function EuclideanDistance(a As Generic.IEnumerable(Of Double), b As Generic.IEnumerable(Of Double)) As Double
        Return EuclideanDistance(a.ToArray, b.ToArray)
    End Function

    <ExportAPI("Euclidean", Info:="Euclidean Distance")>
    <Extension> Public Function EuclideanDistance(a As Double(), b As Double()) As Double
        If a.Length <> b.Length Then
            Return -1.0R
        Else
            Return Math.Sqrt((From i As Integer In a.Sequence Select (a(i) - b(i)) ^ 2).Sum)
        End If
    End Function

    ''' <summary>
    ''' Continues multiply operations.(连续乘法)
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <ExportAPI("PI")>
    <Extension> Public Function PI(data As IEnumerable(Of Double)) As Double
        Dim value As Double = 1
        For Each n In data
            value = value * n
        Next

        Return value
    End Function

    <ExportAPI("RangesAt")>
    <Extension> Public Function RangesAt(n As Double, LowerBound As Double, UpBound As Double) As Boolean
        Return n <= UpBound AndAlso n > LowerBound
    End Function

    <ExportAPI("RangesAt")>
    <Extension> Public Function RangesAt(n As Integer, LowerBound As Double, UpBound As Double) As Boolean
        Return n <= UpBound AndAlso n > LowerBound
    End Function

    <ExportAPI("RangesAt")>
    <Extension> Public Function RangesAt(n As Long, LowerBound As Double, UpBound As Double) As Boolean
        Return n <= UpBound AndAlso n > LowerBound
    End Function

    ''' <summary>
    ''' Root mean square.(均方根)
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <ExportAPI("RMS", Info:="Root mean square")>
    <Extension> Public Function RMS(data As Generic.IEnumerable(Of Double)) As Double
        Dim LQuery = (From n In data.AsParallel Select n ^ 2).ToArray.Sum
        Return Math.Sqrt(LQuery / data.Count)
    End Function

    ''' <summary>
    ''' Returns the PDF value at x for the specified Poisson distribution.
    ''' </summary>
    ''' 
    <ExportAPI("Poisson.PDF", Info:="Returns the PDF value at x for the specified Poisson distribution.")>
    Public Function PoissonPDF(x As Integer, lambda As Double) As Double
        Dim result As Double = Math.Exp(-lambda)
        Dim k As Integer = x
        While k >= 1
            result *= lambda / k
            k -= 1
        End While
        Return result
    End Function
End Module

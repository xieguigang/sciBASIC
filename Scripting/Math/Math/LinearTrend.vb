Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MathApp

    ''' <summary>Linear trend calculation</summary>
    ''' <remarks>
    ''' http://www.codeproject.com/Articles/1101932/Simple-trend-calculation
    ''' </remarks>
    Public Class LinearTrend(Of T) : Inherits ClassObject

        ''' <summary>Slope</summary>
        Public Property Slope As Double

        ''' <summary>Intercept</summary>
        Public Property Intercept As Double

        ''' <summary>Correlation coefficient</summary>
        Public Property Correl As Double

        ''' <summary>R-squared value</summary>
        Public Property R2 As Double

        ''' <summary>Data items</summary>
        Public Property DataItems As ValueItem(Of T)()

        ''' <summary>Trend items</summary>
        Public Property TrendItems As ValueItem(Of T)()

        ''' <summary>Value for the first trend point on X axis</summary>
        Public ReadOnly Property StartPoint As ValueItem(Of T)
            Get
                Dim startItem As ValueItem(Of T)
                startItem = Me.TrendItems.OrderBy(Function(x) x.ConvertedX).FirstOrDefault()
                Return startItem.CreateCopy()
            End Get
        End Property

        ''' <summary>Value for the last trend point on X axis</summary>
        Public ReadOnly Property EndPoint As ValueItem(Of T)
            Get
                Dim endItem As ValueItem(Of T)
                endItem = Me.TrendItems.OrderByDescending(Function(x) x.ConvertedX).FirstOrDefault()
                Return endItem.CreateCopy
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>Calculates the trendline</summary>
        Public Shared Function Calculate(data As List(Of ValueItem(Of T)), newTrendItem As Func(Of ValueItem(Of T))) As LinearTrend(Of T)
            Dim slopeNumerator As Double
            Dim slopeDenominator As Double
            Dim r2Numerator As Double
            Dim r2Denominator As Double
            Dim averageX As Double
            Dim averageY As Double
            Dim trendItem As ValueItem(Of T)

            If data.Count = 0 Then
                Return Nothing
            End If

            ' Calculate slope
            averageX = data.Average(Function(x) x.ConvertedX)
            averageY = data.Average(Function(x) x.Y)
            slopeNumerator = data.Sum(Function(x) (x.ConvertedX - averageX) * (x.Y - averageY))
            slopeDenominator = data.Sum(Function(x) Math.Pow(x.ConvertedX - averageX, 2))

            Dim Slope As Double = slopeNumerator / slopeDenominator

            ' Calculate Intercept
            Dim Intercept As Double = averageY - Slope * averageX

            ' Calculate correlation
            Dim correlDenominator As Double =
            Math.Sqrt(
                data.Sum(Function(x) System.Math.Pow(x.ConvertedX - averageX, 2)) *
                data.Sum(Function(x) System.Math.Pow(x.Y - averageY, 2)))

            Dim Correl As Double = slopeNumerator / correlDenominator
            Dim trendItems As New List(Of ValueItem(Of T))

            ' Calculate trend points
            For Each x As ValueItem(Of T) In data.OrderBy(Function(dataItem) dataItem.ConvertedX)
                If (trendItems.Where(Function(existingItem) existingItem.ConvertedX = x.ConvertedX).FirstOrDefault().isNull) Then
                    trendItem = newTrendItem()
                    trendItem.ConvertedX = x.ConvertedX
                    trendItem.Y = Slope * x.ConvertedX + Intercept
                    trendItems.Add(trendItem)
                End If
            Next

            ' Calculate r-squared value
            r2Numerator = data.Sum(
            Function(dataItem) _
                Math.Pow(dataItem.Y - trendItems.Where(
                Function(calcItem) calcItem.ConvertedX = dataItem.ConvertedX).First().Y, 2))

            r2Denominator = data.Sum(
            Function(dataItem) _
                Math.Pow(dataItem.Y, 2)) - (Math.Pow(data.Sum(
                Function(dataItem) dataItem.Y), 2) / data.Count)

            Dim R2 As Double = 1 - (r2Numerator / r2Denominator)

            Return New LinearTrend(Of T) With {
            .Correl = Correl,
            .DataItems = data.ToArray,
            .Intercept = Intercept,
            .R2 = R2,
            .Slope = Slope,
            .TrendItems = trendItems.ToArray
        }
        End Function
    End Class

    ''' <summary>
    ''' Base class for value items
    ''' </summary>
    Public Structure ValueItem(Of TX)

        ''' <summary>
        ''' The actual value for X
        ''' </summary>
        Public Property X As TX

        ''' <summary>
        ''' The value for X for calculations
        ''' </summary>
        Public Property ConvertedX As Double
            Get
                Return convert(X)
            End Get
            Set(value As Double)
                X = toX(value)
            End Set
        End Property

        ''' <summary>
        ''' Y value of the data item
        ''' </summary>
        Public Property Y As Double

        ''' <summary>
        ''' Creates a copy of the value item
        ''' </summary>
        Public Function CreateCopy() As ValueItem(Of TX)
            Return New ValueItem(Of TX)(convert, toX) With {
                .X = X,
                .Y = Y
            }
        End Function

        Private ReadOnly convert As Func(Of TX, Double), toX As Func(Of Double, TX)

        Public Function isNull() As Boolean
            Return convert Is Nothing OrElse toX Is Nothing
        End Function

        Sub New(convert As Func(Of TX, Double), toX As Func(Of Double, TX))
            Me.convert = convert
            Me.toX = toX
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
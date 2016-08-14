#Region "Microsoft.VisualBasic::b7592aab52d113a3c087b8c20ef02670, ..\VisualBasic_AppFramework\Scripting\Math\Math\LinearTrend.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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

        ''' <summary>
        ''' Calculates the trendline.(使用这个方法来计算趋势曲线拟合)
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="convert">用于数据点的构造函数参数，将目标类型<typeparamref name="T"/>求值</param>
        ''' <param name="toX">用于数据点的构造函数参数，将数值转换为目标类型<typeparamref name="T"/></param>
        ''' <returns></returns>
        Public Shared Function Calculate(data As IEnumerable(Of ValueItem(Of T)), convert As Func(Of T, Double), toX As Func(Of Double, T)) As LinearTrend(Of T)
            Return Calculate(data.ToList, Function() New ValueItem(Of T)(convert, toX))
        End Function

        ''' <summary>
        ''' Calculates the trendline.(使用这个方法来计算趋势曲线拟合)
        ''' </summary>
        ''' <param name="data">原始数据</param>
        ''' <param name="newTrendItem">
        ''' 生成新的数据点的方法，这个函数其实主要是为数据点对象的构造函数提供lambda表达式参数
        ''' </param>
        ''' <returns></returns>
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
                Function(dataItem) Math.Pow(dataItem.Y - trendItems.Where(
                Function(calcItem) calcItem.ConvertedX = dataItem.ConvertedX).First().Y, 2))

            r2Denominator = data.Sum(
                Function(dataItem) Math.Pow(dataItem.Y, 2)) - (Math.Pow(data.Sum(
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

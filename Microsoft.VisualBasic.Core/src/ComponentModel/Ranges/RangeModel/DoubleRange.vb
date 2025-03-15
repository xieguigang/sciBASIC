#Region "Microsoft.VisualBasic::7b31b762b70db2ad8f585b0dcf1304ef, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\RangeModel\DoubleRange.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 369
    '    Code Lines: 199 (53.93%)
    ' Comment Lines: 124 (33.60%)
    '    - Xml Docs: 77.42%
    ' 
    '   Blank Lines: 46 (12.47%)
    '     File Size: 13.60 KB


    '     Class DoubleRange
    ' 
    '         Properties: Length, Max, Min, MinMax
    ' 
    '         Constructor: (+9 Overloads) Sub New
    '         Function: Contains, (+2 Overloads) Enumerate, GetEnumerator, (+3 Overloads) IsInside, (+2 Overloads) IsOverlapping
    '                   (+2 Overloads) ScaleMapping, (+2 Overloads) ToString, TryParse
    '         Operators: *, <>, =, (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' AForge Library
'
' Copyright © Andrew Kirillov, 2006
' andrew.kirillov@gmail.com
'

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Ranges.Model

    ''' <summary>
    ''' Represents a double range with minimum and maximum values
    ''' </summary>
    Public Class DoubleRange : Implements IRangeModel(Of Double)
        Implements Enumeration(Of Double)

        ''' <summary>
        ''' Minimum value
        ''' </summary>
        ''' 
        <XmlAttribute("min")>
        Public Property Min As Double Implements IRangeModel(Of Double).Min

        ''' <summary>
        ''' Maximum value
        ''' </summary>
        '''   
        <XmlAttribute("max")>
        Public Property Max As Double Implements IRangeModel(Of Double).Max

        ''' <summary>
        ''' Length of the range (deffirence between maximum and minimum values)
        ''' </summary>
        ''' 
        Public ReadOnly Property Length As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Max - Min
            End Get
        End Property

        ''' <summary>
        ''' A vector with 2 elements: [min, max]
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property MinMax As Double()
            Get
                Return New Double() {Min, Max}
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="DoubleRange"/> class
        ''' </summary>
        ''' 
        ''' <param name="min">Minimum value of the range</param>
        ''' <param name="max">Maximum value of the range</param>
        <DebuggerStepThrough>
        Public Sub New(min#, max#)
            Me.Min = min
            Me.Max = max
        End Sub

        ''' <summary>
        ''' 从一个任意的实数数组之中构建出一个实数区间范围
        ''' </summary>
        ''' <param name="data"></param>
        Sub New(data As Double())
            If data.Length = 0 Then
                Min = Double.NaN
                Max = Double.NaN
            Else
                Min = data.Min
                Max = data.Max
            End If
        End Sub

        Sub New(data As Integer())
            If data.Length = 0 Then
                Min = Double.NaN
                Max = Double.NaN
            Else
                Min = data.Min
                Max = data.Max
            End If
        End Sub

        ''' <summary>
        ''' 从一个任意的实数向量之中构建出一个实数区间范围
        ''' </summary>
        ''' <param name="vector"></param>
        Sub New(vector As IEnumerable(Of Double))
            Call Me.New(data:=vector.ToArray)
        End Sub

        Sub New(vector As IEnumerable(Of Single))
            Call Me.New(data:=vector.Select(Function(f) CDbl(f)).ToArray)
        End Sub

        Sub New(vector As IEnumerable(Of Integer))
            With vector.ToArray
                If .Length = 0 Then
                    Min = Double.NaN
                    Max = Double.NaN
                Else
                    Min = .Min
                    Max = .Max
                End If
            End With
        End Sub

        Sub New(range As IntRange)
            Call Me.New(range.Min, range.Max)
        End Sub

        Sub New(vec As Vector(Of Double))
            Call Me.New(vec.AsEnumerable)
        End Sub

        ''' <summary>
        ''' Value copy
        ''' </summary>
        ''' <param name="range"></param>
        Sub New(range As DoubleRange)
            Call Me.New(range.Min, range.Max)
        End Sub

        ''' <summary>
        ''' For xml serialization.
        ''' </summary>
        Sub New()
        End Sub

        Public Function Contains(subRange As DoubleRange) As Boolean
            Return IsInside(subRange.Min) AndAlso IsInside(subRange.Max)
        End Function

        ''' <summary>
        ''' [min=xxx, max=xxx]
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 因为进行json序列化的话，因为这个实现了<see cref="IEnumerable(Of T)"/>接口，但是并没有实现Add方法，
        ''' 所以会出错，这里取消使用json来生成<see cref="ToString"/>函数的结果
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return ToString("G4")
        End Function

        Public Overloads Function ToString(format As String) As String
            Return $"[min={Min.ToString(format)}, max={Max.ToString(format)}]"
        End Function

        ''' <summary>
        ''' Check if the specified value is inside this range
        ''' </summary>
        ''' 
        ''' <param name="x">Value to check</param>
        ''' 
        ''' <returns><b>True</b> if the specified value is inside this range or
        ''' <b>false</b> otherwise.</returns>
        ''' 
        Public Function IsInside(x As Double) As Boolean Implements IRangeModel(Of Double).IsInside
            Return ((x >= Min) AndAlso (x <= Max))
        End Function

        ''' <summary>
        ''' Check if the specified <paramref name="range"/> is inside this range.
        ''' (如果函数参数<paramref name="range"/>在当前的这个range之中，则返回真)
        ''' </summary>
        ''' <param name="range">Range to check</param>
        ''' <returns>
        ''' + <b>True</b> if the specified input <paramref name="range"/> parameter is inside this range or
        ''' + <b>false</b> otherwise.</returns>
        Public Function IsInside(range As DoubleRange) As Boolean
            Return ((IsInside(range.Min)) AndAlso (IsInside(range.Max)))
        End Function

        ''' <summary>
        ''' Check if the specified range overlaps with this range
        ''' </summary>
        ''' 
        ''' <param name="range">Range to check for overlapping</param>
        ''' 
        ''' <returns><b>True</b> if the specified range overlaps with this range or
        ''' <b>false</b> otherwise.</returns>
        ''' 
        Public Function IsOverlapping(range As DoubleRange) As Boolean
            Return ((IsInside(range.Min)) OrElse (IsInside(range.Max)))
        End Function

        Public Function IsInside(range As IRangeModel(Of Double)) As Boolean Implements IRangeModel(Of Double).IsInside
            Return ((IsInside(range.Min)) AndAlso (IsInside(range.Max)))
        End Function

        Public Function IsOverlapping(range As IRangeModel(Of Double)) As Boolean Implements IRangeModel(Of Double).IsOverlapping
            Return ((IsInside(range.Min)) OrElse (IsInside(range.Max)))
        End Function

        '<MethodImpl(MethodImplOptions.AggressiveInlining)>
        'Public Shared Widening Operator CType(exp As String) As DoubleRange
        '    Dim r As New DoubleRange
        '    Call exp.Parser(r.Min, r.Max)
        '    Return r
        'End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data#()) As DoubleRange
            With data
                Return New DoubleRange(min:= .Min, max:= .Max)
            End With
        End Operator

#If NET_48 Or NETCOREAPP Then

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(tuple As (min#, max#)) As DoubleRange
            Return New DoubleRange(tuple.min, tuple.max)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(tuple As (min!, max!)) As DoubleRange
            Return New DoubleRange(tuple.min, tuple.max)
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(tuple As (min&, max&)) As DoubleRange
            Return New DoubleRange(tuple.min, tuple.max)
        End Operator

#End If

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(vector As Vector(Of Double)) As DoubleRange
            If vector.Length = 0 Then
                Return New DoubleRange(0, 0)
            Else
                Return New DoubleRange(vector.Min, vector.Max)
            End If
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As VectorShadows(Of Single)) As DoubleRange
            Return data _
                .Select(Function(s) CDbl(s)) _
                .ToArray
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(data As Single()) As DoubleRange
            Return New DoubleRange(data.Min, data.Max)
        End Operator

        ''' <summary>
        ''' Value in this range or not?
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="range"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator Like(x As Double, range As DoubleRange) As Boolean
            Return range.IsInside(x)
        End Operator

        ''' <summary>
        ''' Scale numeric range
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="factor#"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator *(range As DoubleRange, factor#) As DoubleRange
            With range
                Dim delta = (.Length * factor - .Length) / 2
                Dim ar As Double() = {
                    .Min - delta,
                    .Max + delta
                }

                Return New DoubleRange(ar)
            End With
        End Operator

        ''' <summary>
        ''' 这个函数需要通过一个返回结果的元素个数参数来计算出step步长
        ''' </summary>
        ''' <param name="n%">所返回来的数组的元素的个数</param>
        ''' <returns></returns>
        Public Function Enumerate(n%) As Double()
            If Length = 0 Then
                Return {}
            Else
                ' 因为double类型的精度问题
                ' 有些时候返回来的元素数量可能不是n个？
                ' 极有可能是n+1个？
                Return Enumerate(Length / n).Take(n).ToArray
            End If
        End Function

        Public Iterator Function Enumerate(resolution#) As IEnumerable(Of Double)
            For x As Double = Min To Max Step resolution
                Yield x
            Next
        End Function

        ''' <summary>
        ''' Transform a numeric value in this <see cref="DoubleRange"/> into 
        ''' target numeric range: ``<paramref name="valueRange"/>``.
        ''' (将当前的范围内的一个实数映射到另外的一个范围内的实数区间之中)
        ''' </summary>
        ''' <param name="x#">A numeric value in this <see cref="DoubleRange"/></param>
        ''' <param name="valueRange"></param>
        ''' <returns></returns>
        Public Function ScaleMapping(x#, valueRange As DoubleRange) As Double
            Dim percent# = (x - Min) / Length
            Dim value# = percent * valueRange.Length + valueRange.Min
            Return value
        End Function

        ''' <summary>
        ''' Transform a numeric value in this <see cref="DoubleRange"/> into 
        ''' target numeric range: ``<paramref name="valueRange"/>``.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="valueRange">levels in integer values.</param>
        ''' <returns></returns>
        Public Function ScaleMapping(x As Double, valueRange As IntRange) As Integer
            Dim percent# = (x - Min) / Length
            Dim value# = percent * valueRange.Interval + valueRange.Min
            Return CInt(value)
        End Function

        Public Overridable Iterator Function GetEnumerator() As IEnumerator(Of Double) Implements Enumeration(Of Double).GenericEnumerator
            For Each x In Me.Enumerate(100)
                Yield x
            Next
        End Function

        Public Shared Operator =(range As DoubleRange, value#) As Boolean
            If range Is Nothing AndAlso value = 0 Then
                ' 假若将doublerange看作为double类型的数值的话，则数值类型的Nothing值为0，
                ' 所以在这里将Nothing等价于右边的value 0
                Return True
            Else
                Return range.Length = 0 AndAlso range.Min = value
            End If
        End Operator

        Public Shared Operator <>(range As DoubleRange, value#) As Boolean
            Return Not range = value
        End Operator

        Public Shared Function TryParse(expression As String) As DoubleRange
            If expression.StringEmpty Then
                Return Nothing
            End If

            Try
                Dim r As New DoubleRange
                Call expression.Parser(r.Min, r.Max)
                Return r
            Catch ex As Exception
                Return Nothing
            End Try
        End Function
    End Class
End Namespace

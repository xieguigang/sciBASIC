#Region "Microsoft.VisualBasic::b96520dcbb2e87616b54416b362416e1, Microsoft.VisualBasic.Core\src\ComponentModel\Ranges\RangeModel\IntRange.vb"

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

    '   Total Lines: 165
    '    Code Lines: 67 (40.61%)
    ' Comment Lines: 77 (46.67%)
    '    - Xml Docs: 74.03%
    ' 
    '   Blank Lines: 21 (12.73%)
    '     File Size: 6.04 KB


    '     Class IntRange
    ' 
    '         Properties: Interval, Max, Min
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, (+3 Overloads) IsInside, (+2 Overloads) IsOverlapping, ScaleMapping
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' AForge Library
'
' Copyright © Andrew Kirillov, 2006
' andrew.kirillov@gmail.com
'

Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Ranges.Model

    ''' <summary>
    ''' Represents an <see cref="Integer"/> range with minimum and maximum values
    ''' </summary>
    Public Class IntRange
        Implements IRangeModel(Of Integer)
        Implements IEnumerable(Of Integer)

        ''' <summary>
        ''' Minimum value
        ''' </summary>
        Public Property Min As Integer Implements IRangeModel(Of Integer).Min

        ''' <summary>
        ''' Maximum value
        ''' </summary>
        Public Property Max As Integer Implements IRangeModel(Of Integer).Max

        ''' <summary>
        ''' Length of the range (deffirence between maximum and minimum values)
        ''' </summary>
        Public ReadOnly Property Interval As Integer
            Get
                Return Max - Min
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="IntRange"/> class
        ''' </summary>
        '''
        ''' <param name="min">Minimum value of the range</param>
        ''' <param name="max">Maximum value of the range</param>
        Public Sub New(min%, max%)
            Me.Min = min
            Me.Max = max
        End Sub

        ''' <summary>
        ''' 这个构造函数之中会自动求出最大值和最小值
        ''' </summary>
        ''' <param name="source"></param>
        Sub New(source As IEnumerable(Of Integer))
            Dim minmax As Integer() = IntRange.MinMax(source)

            Min = minmax(0)
            Max = minmax(1)
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' <see cref="DoubleRange.ToString()"/>
        ''' </remarks>
        Public Overrides Function ToString() As String
            Return $"[min={Min}, max={Max}]"
        End Function

        ''' <summary>
        ''' Check if the specified value ``<paramref name="x"/>`` is inside this range
        ''' </summary>
        ''' <param name="x">Value to check</param>
        ''' <returns><b>True</b> if the specified value is inside this range or
        ''' <b>false</b> otherwise.</returns>
        Public Function IsInside(x As Integer) As Boolean Implements IRangeModel(Of Integer).IsInside
            Return ((x >= Min) AndAlso (x <= Max))
        End Function

        ''' <summary>
        ''' Check if the specified range is inside this range
        ''' </summary>
        '''
        ''' <param name="range">Range to check</param>
        '''
        ''' <returns><b>True</b> if the specified range is inside this range or
        ''' <b>false</b> otherwise.</returns>
        '''
        Public Function IsInside(range As IntRange) As Boolean
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
        Public Function IsOverlapping(range As IntRange) As Boolean
            Return ((IsInside(range.Min)) OrElse (IsInside(range.Max)))
        End Function

        Public Function IsInside(range As IRangeModel(Of Integer)) As Boolean Implements IRangeModel(Of Integer).IsInside
            Return ((IsInside(range.Min)) AndAlso (IsInside(range.Max)))
        End Function

        Public Function IsOverlapping(range As IRangeModel(Of Integer)) As Boolean Implements IRangeModel(Of Integer).IsOverlapping
            Return ((IsInside(range.Min)) OrElse (IsInside(range.Max)))
        End Function

        ''' <summary>
        ''' Transform a numeric value in this <see cref="IntRange"/> into 
        ''' target numeric range: ``<paramref name="valueRange"/>``.
        ''' (将当前的范围内的一个实数映射到另外的一个范围内的实数区间之中)
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="valueRange"></param>
        ''' <returns></returns>
        Public Function ScaleMapping(x%, valueRange As IntRange) As Double
            Dim percent# = (x - Min) / Interval
            Dim value# = percent * valueRange.Interval + valueRange.Min
            Return value
        End Function

        ''' <summary>
        ''' 枚举出这个数值范围内的所有整数值，步长为1
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of Integer) Implements IEnumerable(Of Integer).GetEnumerator
            For i As Integer = Min To Max
                Yield i
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Shared Widening Operator CType(exp$) As IntRange
            Dim r As New IntRange
            Call exp.Parser(r.Min, r.Max)
            Return r
        End Operator

        ' 2017-10-6
        ' 因为下面的这个隐式转换操作符会和Vector的Item属性产生冲突，所以在这里将这个操作符移除掉
        'Public Shared Widening Operator CType(values%()) As IntRange
        '    With values
        '        Return New IntRange(.Min, .Max)
        '    End With
        'End Operator

        Public Shared Widening Operator CType(list As List(Of Integer)) As IntRange
            With list
                Return New IntRange(.Min, .Max)
            End With
        End Operator

        Public Shared Function MinMax(ints As IEnumerable(Of Integer)) As Integer()
            Dim min As Integer = Integer.MaxValue
            Dim max As Integer = Integer.MinValue

            For Each i As Integer In ints
                If i > max Then
                    max = i
                End If
                If i < min Then
                    min = i
                End If
            Next

            Return {min, max}
        End Function
    End Class
End Namespace

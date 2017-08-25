#Region "Microsoft.VisualBasic::9cdacf6a6608c81e347cec8af9688956, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Ranges\IntRange.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

' AForge Library
'
' Copyright © Andrew Kirillov, 2006
' andrew.kirillov@gmail.com
'

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.Ranges

    ''' <summary>
    ''' Represents an integer range with minimum and maximum values
    ''' </summary>
    Public Class IntRange : Inherits ClassObject
        Implements IRanges(Of Integer)
        Implements IEnumerable(Of Integer)

        ''' <summary>
        ''' Minimum value
        ''' </summary>
        Public Property Min() As Integer Implements IRanges(Of Integer).Min

        ''' <summary>
        ''' Maximum value
        ''' </summary>
        Public Property Max() As Integer Implements IRanges(Of Integer).Max

        ''' <summary>
        ''' Length of the range (deffirence between maximum and minimum values)
        ''' </summary>
        Public ReadOnly Property Length() As Integer
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
        Public Sub New(min As Integer, max As Integer)
            Me.Min = min
            Me.Max = max
        End Sub

        Sub New(source As IEnumerable(Of Integer))
            Dim array As Integer() = source.ToArray
            Me.Min = array.Min
            Me.Max = array.Max
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
        ''' Check if the specified value is inside this range
        ''' </summary>
        '''
        ''' <param name="x">Value to check</param>
        '''
        ''' <returns><b>True</b> if the specified value is inside this range or
        ''' <b>false</b> otherwise.</returns>
        '''
        Public Function IsInside(x As Integer) As Boolean Implements IRanges(Of Integer).IsInside
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

        Public Function IsInside(range As IRanges(Of Integer)) As Boolean Implements IRanges(Of Integer).IsInside
            Return ((IsInside(range.Min)) AndAlso (IsInside(range.Max)))
        End Function

        Public Function IsOverlapping(range As IRanges(Of Integer)) As Boolean Implements IRanges(Of Integer).IsOverlapping
            Return ((IsInside(range.Min)) OrElse (IsInside(range.Max)))
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
    End Class
End Namespace

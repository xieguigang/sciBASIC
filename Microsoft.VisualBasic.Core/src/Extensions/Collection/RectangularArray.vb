#Region "Microsoft.VisualBasic::911982a90888fd41c3200cbfee786704, Microsoft.VisualBasic.Core\src\Extensions\Collection\RectangularArray.vb"

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

    '   Total Lines: 135
    '    Code Lines: 82 (60.74%)
    ' Comment Lines: 27 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 26 (19.26%)
    '     File Size: 4.62 KB


    '     Class RectangularArray
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CopyOf, Cubic, CubicMatrix, GlobalRange, (+3 Overloads) Matrix
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace ComponentModel.Collection

    Public NotInheritable Class RectangularArray

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Make deep copy of the rectangular array value
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="m"></param>
        ''' <returns></returns>
        Public Shared Function CopyOf(Of T)(ByRef m As T()()) As T()()
            Dim copy As T()() = New T(m.Length - 1)() {}

            For i As Integer = 0 To m.Length - 1
                copy(i) = m(i).ToArray
            Next

            Return copy
        End Function

        Public Shared Function GlobalRange(m As IEnumerable(Of Double())) As (Min As Double, Max As Double)
            Dim min As Double = Double.MaxValue
            Dim max As Double = Double.MinValue
            Dim range As DoubleRange

            For Each row As Double() In m
                range = row.Range

                If range.Min < min Then min = range.Min
                If range.Max > max Then max = range.Max
            Next

            Return (min, max)
        End Function

        ''' <summary>
        ''' Create an empty matrix with m row and n cols.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="m">m Rows</param>
        ''' <param name="n">n Cols</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (生成一个有m行n列的矩阵，但是是使用数组来表示的)
        ''' </remarks>
        Public Shared Function Matrix(Of T)(m%, n%) As T()()
            Dim x As T()() = New T(m - 1)() {}

            For i As Integer = 0 To m - 1
                x(i) = New T(n - 1) {}
            Next

            Return x
        End Function

        ''' <summary>
        ''' Create an empty matrix with m row and n cols.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="m">m Rows</param>
        ''' <param name="n">n Cols</param>
        ''' <param name="s">fill the generated matrix this scalar value</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' (生成一个有m行n列的矩阵，但是是使用数组来表示的)
        ''' </remarks>
        Public Shared Function Matrix(Of T)(m%, n%, s As T) As T()()
            Dim x As T()() = New T(m - 1)() {}
            Dim template As T() = New T(n - 1) {}
            Dim v As T()

            For i As Integer = 0 To n - 1
                template(i) = s
            Next

            For i As Integer = 0 To m - 1
                v = New T(n - 1) {}
                Array.ConstrainedCopy(template, Scan0, v, Scan0, n)
                x(i) = v
            Next

            Return x
        End Function

        Public Shared Function Cubic(Of T)(size1 As Integer, size2 As Integer, size3 As Integer) As T()()()
            Dim x = New T(size1 - 1)()() {}

            For array1 As Integer = 0 To size1 - 1
                x(array1) = New T(size2 - 1)() {}
                If size3 > -1 Then
                    For array2 As Integer = 0 To size2 - 1
                        x(array1)(array2) = New T(size3 - 1) {}
                    Next
                End If
            Next

            Return x
        End Function

        Public Shared Function Matrix(type As Type, m%, n%) As Array
            Dim newMatrix As Array = Array.CreateInstance(type.MakeArrayType, m)

            For i As Integer = 0 To m - 1
                Call newMatrix.SetValue(Array.CreateInstance(type, n), i)
            Next

            Return newMatrix
        End Function

        Public Shared Function CubicMatrix(Of T)(size1 As Integer, size2 As Integer, size3 As Integer, size4 As Integer) As T()()()()
            Dim cm = New T(size1 - 1)()()() {}

            For array1 = 0 To size1 - 1
                cm(array1) = New T(size2 - 1)()() {}
                If size3 > -1 Then
                    For array2 = 0 To size2 - 1
                        cm(array1)(array2) = New T(size3 - 1)() {}
                        If size4 > -1 Then
                            For array3 = 0 To size3 - 1
                                cm(array1)(array2)(array3) = New T(size4 - 1) {}
                            Next
                        End If
                    Next
                End If
            Next

            Return cm
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::4e391e604811f7fab8ef519ea049876b, Data_science\MachineLearning\RestrictedBoltzmannMachine\PrettyPrint.vb"

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

    '   Total Lines: 68
    '    Code Lines: 55 (80.88%)
    ' Comment Lines: 3 (4.41%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 10 (14.71%)
    '     File Size: 2.48 KB


    '     Class PrettyPrint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) toPixelBox, (+2 Overloads) ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace utils

    ''' <summary>
    ''' Created by kenny on 5/13/14.
    ''' </summary>
    Public Class PrettyPrint

        Private Sub New()
        End Sub

        Public Overloads Shared Function ToString(array As Object()) As String
            Dim stringBuilder As StringBuilder = New StringBuilder()
            stringBuilder.Append("[")
            For i = 0 To array.Length - 1
                stringBuilder.Append(array(i))
                If i < array.Length - 1 Then
                    stringBuilder.Append(ChrW(10))
                End If
            Next
            stringBuilder.Append("]")
            Return stringBuilder.ToString()
        End Function

        Public Overloads Shared Function ToString(arrays As Double()()) As String
            Dim stringBuilder As StringBuilder = New StringBuilder()
            stringBuilder.AppendLine("[")
            For i = 0 To arrays.Length - 1
                stringBuilder.Append("   " & arrays(i).GetJson())
                stringBuilder.Append(ChrW(10))
            Next
            stringBuilder.Append("]")
            Return stringBuilder.ToString()
        End Function

        Public Shared Function toPixelBox(arrays As Double()(), threshold As Double) As String
            Dim stringBuilder As StringBuilder = New StringBuilder()
            For Each array In arrays
                For i = 0 To array.Length - 1
                    If array(i) >= threshold Then
                        stringBuilder.Append("■")
                    Else
                        stringBuilder.Append("□")
                    End If
                Next
                stringBuilder.Append(ChrW(10))
            Next
            Return stringBuilder.ToString()
        End Function

        Public Shared Function toPixelBox(array As Double(), columnSize As Integer, threshold As Double) As String

            Dim rowSize As Integer = array.Length / columnSize

            Dim matrix = RectangularArray.Matrix(Of Double)(rowSize, columnSize)
            For i = 0 To rowSize - 1
                For j = 0 To columnSize - 1
                    matrix(i)(j) = array(i * columnSize + j)
                Next
            Next
            Return toPixelBox(matrix, threshold)
        End Function
    End Class

End Namespace

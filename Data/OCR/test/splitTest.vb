#Region "Microsoft.VisualBasic::1515647ca552f9fe3d0b014160f76fd5, sciBASIC#\Data\OCR\test\splitTest.vb"

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

    '   Total Lines: 41
    '    Code Lines: 28
    ' Comment Lines: 2
    '   Blank Lines: 11
    '     File Size: 1.08 KB


    ' Module splitTest
    ' 
    '     Function: asChar
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Module splitTest
    Sub Main()

        Dim bitmap As New List(Of Integer())
        Dim i As int = 0

        For y As Integer = 0 To 20 - 1
            Dim line As New List(Of Integer)

            For x As Integer = 0 To 20 - 1
                line.Add(++i)
            Next

            bitmap.Add(line)
        Next

        Dim vector = bitmap.IteratesALL.AsVector
        Dim target As Vector = {11, 12, 13, 14, 15}

        'Dim local As New GSW(Of Double)(target, vector, Function(a, b) If(a = b, 1, 0), AddressOf asChar)
        'Dim matches = local.GetMatches(local.MaxScore * 0.95).Select(Function(m) m - 1).ToArray



        Pause()
    End Sub

    Private Function asChar(d As Double) As Char
        If d = 0R OrElse d = 1.0R Then
            Return d.ToString.First
        ElseIf d = -1.0R Then
            Return "*"c
        Else
            Return "7"c
        End If
    End Function
End Module

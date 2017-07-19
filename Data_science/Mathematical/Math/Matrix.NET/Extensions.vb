#Region "Microsoft.VisualBasic::06bc2b9f519fc348f5420b9369454e60, ..\sciBASIC#\Data_science\Mathematical\Math\Matrix.NET\Extensions.vb"

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

Namespace Matrix

    Public Module Extensions

        Public Function size(M As GeneralMatrix, d%) As Integer
            If d = 1 Then
                Return M.RowDimension
            ElseIf d = 2 Then
                Return M.ColumnDimension
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>Generate matrix with random elements</summary>
        ''' <param name="m">   Number of rows.
        ''' </param>
        ''' <param name="n">   Number of colums.
        ''' </param>
        ''' <returns>     An m-by-n matrix with uniformly distributed random elements.
        ''' </returns>

        Public Function rand(m%, n%) As GeneralMatrix
            With New Random()
                Dim A As New GeneralMatrix(m, n)
                Dim X As Double()() = A.Array

                For i As Integer = 0 To m - 1
                    For j As Integer = 0 To n - 1
                        X(i)(j) = .NextDouble()
                    Next
                Next

                Return A
            End With
        End Function
    End Module
End Namespace

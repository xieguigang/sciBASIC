#Region "Microsoft.VisualBasic::cec6d930ae488206ead5b8daa45913ae, ..\sciBASIC#\Data\DataFrame\DATA\Excel\Coordinates.vb"

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

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Excel

    Public Module Coordinates

        <Extension>
        Public Function CellValue(data As IO.File, c As String) As String
            With Coordinates.Index(c)
                Return data.Cell(.X, .Y)
            End With
        End Function

        ReadOnly AZ As New Index(Of Char)("ABCDEFGHIJKLMNOPQRSTUVWXYZ")
        ReadOnly ZERO% = Asc("A"c) - 1

        Public Function Index(c$) As Point
            Dim Y As New List(Of Char)
            Dim X%

            For Each [char] As Char In c
                If AZ.IndexOf([char]) > -1 Then
                    Y.Add([char])
                Else
                    X = CInt(Val(c.Skip(Y.Count).CharString))
                    Exit For
                End If
            Next

            Return New Point(X, Y.YValue)
        End Function

        ''' <summary>
        ''' 也就是获取得到列的顶点编号
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <Extension> Public Function YValue(x As IEnumerable(Of Char)) As Integer
            Dim value#
            Dim power% = 0

            For Each c In x.Reverse
                value += (Asc(c) - ZERO) * (26 ^ power)
            Next

            Return CInt(value)
        End Function

        <Extension>
        Public Function RangeSelects(data As IO.File, range As String) As String()
            Throw New NotImplementedException
        End Function
    End Module
End Namespace

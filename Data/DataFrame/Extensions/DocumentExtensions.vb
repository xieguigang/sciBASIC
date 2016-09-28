#Region "Microsoft.VisualBasic::3156855c09508a58c8fc083ecb8b150d, ..\visualbasic_App\Data\DataFrame\Extensions\DocumentExtensions.vb"

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

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.DocumentStream

Public Module DocumentExtensions

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="cols"><see cref="File.Columns"/> filtering results.</param>
    ''' <returns></returns>
    <Extension>
    Public Function JoinColumns(cols As IEnumerable(Of String())) As DocumentStream.File
        Dim array$()() = cols.ToArray
        Dim out As New File

        For i As Integer = 0 To array.First.Length - 1
            Dim ind As Integer = i
            out += New RowObject(array.Select(Function(x) x(ind)))
        Next

        Return out
    End Function

    <Extension>
    Public Function Apply(ByRef row As RowObject, action As Func(Of String, String), Optional skip As Integer = 0) As RowObject
        For i As Integer = skip To row._innerColumns.Count - 1
            row._innerColumns(i) = action(row._innerColumns(i))
        Next

        Return row
    End Function
End Module

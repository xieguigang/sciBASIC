#Region "Microsoft.VisualBasic::627cca4c7461fb25529cef9b73b1af8a, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\TablePrinter\ConsoleTableBaseData.vb"

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

    '   Total Lines: 63
    '    Code Lines: 43 (68.25%)
    ' Comment Lines: 5 (7.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (23.81%)
    '     File Size: 2.02 KB


    '     Class ConsoleTableBaseData
    ' 
    '         Properties: Column, Rows
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+2 Overloads) AppendLine, FromColumnHeaders, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Terminal.TablePrinter

    Public Class ConsoleTableBaseData

        Public Property Column As List(Of Object)
        Public Property Rows As List(Of Object())

        Sub New()
        End Sub

        Sub New(ParamArray headers As String())
            Column = headers.Select(Function(si) CObj(si)).AsList
        End Sub

        Sub New(headers As String(), rows As IEnumerable(Of String()))
            Call Me.New(headers)

            Me.Rows = New List(Of Object())

            For Each row As String() In rows.SafeQuery
                Call AppendLine(DirectCast(row, IEnumerable))
            Next
        End Sub

        ''' <summary>
        ''' append one row data
        ''' </summary>
        ''' <param name="line">the row data collection, general data</param>
        ''' <returns></returns>
        Public Function AppendLine(line As IEnumerable) As ConsoleTableBaseData
            Rows.Add((From x As Object In line Select x).ToArray)
            Return Me
        End Function

        Public Function AppendLine(ParamArray row As Object()) As ConsoleTableBaseData
            Rows.Add(row)
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return Column.ToStringArray.GetJson
        End Function

        Public Shared Function FromColumnHeaders(ParamArray headers As String()) As ConsoleTableBaseData
            Dim empty As New ConsoleTableBaseData With {
                .Column = New List(Of Object),
                .Rows = New List(Of Object())
            }

            For Each name As String In headers
                Call empty.Column.Add(name)
            Next

            Return empty
        End Function

    End Class
End Namespace

#Region "Microsoft.VisualBasic::8835570fb5f409d8d55dee17c40b0aa7, Data\BinaryData\BinaryData\SQLite3\Schema\Schema.vb"

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

    '     Class Schema
    ' 
    '         Properties: Columns
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ParseColumns
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ManagedSqlite.Core

    Public Class Schema

        Public Property Columns As NamedValue(Of String)()

        Sub New(columns As String())
            Me.Columns = ParseColumns(columns).ToArray
        End Sub

        Private Iterator Function ParseColumns(columns As String()) As IEnumerable(Of NamedValue(Of String))
            Dim tokens As String()
            Dim field As NamedValue(Of String)

            For Each column As String In columns
                tokens = column.StringSplit("\s+")
                field = New NamedValue(Of String) With {
                    .Name = tokens(Scan0),
                    .Value = tokens(1)
                }

                Yield field
            Next
        End Function
    End Class
End Namespace

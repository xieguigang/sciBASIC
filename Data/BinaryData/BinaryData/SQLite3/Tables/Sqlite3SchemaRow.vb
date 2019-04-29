#Region "Microsoft.VisualBasic::35e93eaee0f76bb39601df4b008bb983, Data\BinaryData\BinaryData\SQLite3\Tables\Sqlite3SchemaRow.vb"

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

    '     Class Sqlite3SchemaRow
    ' 
    '         Properties: Name, RootPage, Sql, TableName, Type
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ManagedSqlite.Core.Tables

    Public Class Sqlite3SchemaRow

        Public Property Type() As String
        Public Property Name() As String
        Public Property TableName() As String
        Public Property RootPage() As UInteger
        Public Property Sql() As String

        Public Overrides Function ToString() As String
            Return Sql
        End Function

        Public Function ParseSchema() As Schema
            Dim columns = Sql.GetStackValue("(", ")").StringSplit("\s*,\s*")
            Return New Schema(columns)
        End Function

    End Class
End Namespace

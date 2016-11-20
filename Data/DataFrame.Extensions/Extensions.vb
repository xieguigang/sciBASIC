#Region "Microsoft.VisualBasic::1cce6dcb2d405598edb42927dcdbccb8, ..\sciBASIC#\Data\DataFrame.Extensions\Extensions.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Module SchemasAPI

    <Extension>
    Public Function SaveData(Of T As Class)(source As IEnumerable(Of T), DIR As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean
        Dim schema As Schema = Schema.GetSchema(Of T)
        Dim type As Type = GetType(T)
        Dim IO As [Class] = [Class].GetSchema(type)
        Dim i As New Uid

        Using writer As New Writer(IO, DIR, encoding)
            For Each x As T In source
                Call writer.WriteRow(x, +i)
            Next
        End Using

        Return schema.GetJson(True).SaveTo(DIR & "/" & Schema.DefaultName)
    End Function
End Module

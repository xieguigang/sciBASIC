#Region "Microsoft.VisualBasic::0723d0ed2e2c569bb018511a68c9a99d, Microsoft.VisualBasic.Core\ComponentModel\DataSource\SchemaMaps\Schema.vb"

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
'         Properties: Fields, SchemaName
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' Schema for two dimension table.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Schema(Of T As Attribute)

        Public Property SchemaName As String
        Public Property Fields As BindProperty(Of T)()

        Sub New()
        End Sub

        Sub New(type As Type, Optional explict As Boolean = False)
            Fields = type.GetFields(Of T)(Function(o) o.ToString, explict)
            SchemaName = type.Name
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{SchemaName}: {Fields.Keys.GetJson}]"
        End Function
    End Class
End Namespace

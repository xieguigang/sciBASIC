#Region "Microsoft.VisualBasic::fe337701f9fe401632dfe8f06d702e44, Microsoft.VisualBasic.Core\Language\Language\JavaScript\JavaScriptObject.vb"

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

    '     Class JavaScriptObject
    ' 
    '         Properties: Accessor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace Language.JavaScript

    Public Class JavaScriptObject

        Private members As Dictionary(Of String, BindProperty(Of DataFrameColumnAttribute))

        Public Property Accessor(memberName As String) As Object
            Get
                If members.ContainsKey(memberName) Then
                    Return members(memberName).GetValue(Me)
                Else
                    ' Returns undefined in javascript
                    Return Nothing
                End If
            End Get
            Set(value As Object)
                If members.ContainsKey(memberName) Then
                    members(memberName).SetValue(Me, value)
                Else
                    ' 添加一个新的member？
                    Throw New NotImplementedException
                End If
            End Set
        End Property

    End Class
End Namespace

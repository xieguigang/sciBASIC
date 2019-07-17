#Region "Microsoft.VisualBasic::19bfdec5cef80f52096f361a1993ed21, My\JavaScript\JavaScriptObject.vb"

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

    '     Interface IJavaScriptObjectAccessor
    ' 
    '         Properties: Accessor
    ' 
    '     Class JavaScriptObject
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace My.JavaScript

    Public Interface IJavaScriptObjectAccessor
        Default Property Accessor(name As String) As Object
    End Interface

    Public Class JavaScriptObject : Implements IEnumerable(Of String), IJavaScriptObjectAccessor

        Dim members As New Dictionary(Of String, BindProperty(Of DataFrameColumnAttribute))

        ''' <summary>
        ''' 只针对Public的属性或者字段有效
        ''' </summary>
        ''' <param name="memberName"></param>
        ''' <returns></returns>
        Default Public Property Accessor(memberName As String) As Object Implements IJavaScriptObjectAccessor.Accessor
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

        ''' <summary>
        ''' 如果存在无参数的函数，则也会被归类为只读属性？
        ''' </summary>
        Sub New()
            Dim type As Type = MyClass.GetType
            Dim properties As PropertyInfo() = type.GetProperties(PublicProperty).ToArray
            Dim fields As FieldInfo() = type.GetFields(PublicProperty).ToArray

            For Each prop As PropertyInfo In properties
                members(prop.Name) = New BindProperty(Of DataFrameColumnAttribute)(prop)
            Next
            For Each field As FieldInfo In fields
                members(field.Name) = New BindProperty(Of DataFrameColumnAttribute)(field)
            Next
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
            For Each key As String In members.Keys
                Yield key
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace

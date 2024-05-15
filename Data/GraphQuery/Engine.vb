#Region "Microsoft.VisualBasic::6f41769dd90e7e40fa53836d10219534, Data\GraphQuery\Engine.vb"

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

    '   Total Lines: 133
    '    Code Lines: 103
    ' Comment Lines: 5
    '   Blank Lines: 25
    '     File Size: 4.52 KB


    ' Class Engine
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+3 Overloads) Execute, QueryObject, QueryObjectArray, QueryValue, QueryValueArray
    ' 
    '     Sub: addPackage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphQuery.TextParser
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Html
Imports Microsoft.VisualBasic.MIME.Html.Document

''' <summary>
''' the engine of run graph query
''' </summary>
Public Class Engine

    ReadOnly funcs As New Dictionary(Of String, ParserFunction)

    Sub New()
        Call addPackage(GetType(TextParser.BaseInvoke))
        Call addPackage(GetType(TextParser.Html))
        Call addPackage(GetType(TextParser.LINQ))
    End Sub

    Private Sub addPackage(pkg As Type)
        Dim method As MethodInfo() = pkg.GetMethods
        Dim entry As ExportAPIAttribute

        For Each api As MethodInfo In method.Where(Function(m) m.IsStatic)
            entry = api.GetCustomAttribute(Of ExportAPIAttribute)

            If Not entry Is Nothing Then
                funcs(entry.Name) = New InternalInvoke With {
                    .name = entry.Name,
                    .method = api
                }
            End If
        Next
    End Sub

    Public Function Execute(document As InnerPlantText, func As String, param As String(), isArray As Boolean) As InnerPlantText
        If Not funcs.ContainsKey(func) Then
            Throw New KeyNotFoundException(func)
        Else
            Return funcs(func).GetToken(document, param, isArray)
        End If
    End Function

    Public Function Execute(document As XElement, query As Query) As JsonElement
        Return Execute(document.CreateDocument, query)
    End Function

    Public Function Execute(document As HtmlElement, query As Query) As JsonElement
        If Not query.members.IsNullOrEmpty Then
            ' object
            Dim subDocument As InnerPlantText
            Dim obj As JsonElement

            If query.parser Is Nothing Then
                subDocument = document
            Else
                subDocument = query.parser.Parse(document, query.isArray, Me)
            End If

            If subDocument.GetType Is GetType(InnerPlantText) Then
                obj = New JsonObject
            ElseIf query.isArray Then
                obj = QueryObjectArray(subDocument, query)
            Else
                obj = QueryObject(subDocument, query)
            End If

            Return obj
        Else
            If query.isArray Then
                Return QueryValueArray(document, query)
            Else
                ' value
                Return QueryValue(document, query)
            End If
        End If
    End Function

    Private Function QueryValueArray(document As HtmlElement, query As Query) As JsonArray
        Dim array As New JsonArray

        For Each item In document.HtmlElements
            array.Add(QueryValue(item, query))
        Next

        Return array
    End Function

    Private Function QueryValue(document As HtmlElement, query As Query) As JsonValue
        Dim value As InnerPlantText = query.parser.Parse(document, query.isArray, Me)
        Dim valStr As String = value.GetPlantText
        Dim json As New JsonValue(valStr)

        Return json
    End Function

    Private Function QueryObjectArray(document As HtmlElement, query As Query) As JsonArray
        Dim array As New JsonArray

        If query.members.Length = 1 AndAlso query.members(Scan0).name = "@array" Then
            query = query.members(Scan0)

            For Each item As InnerPlantText In document.HtmlElements
                If item.GetType Is GetType(InnerPlantText) Then
                    item = New HtmlElement With {
                        .InnerText = item.GetPlantText,
                        .TagName = "na",
                        .Attributes = {AutoContext.Attribute}
                    }
                End If

                array.Add(Execute(item, query))
            Next
        Else
            For Each item In document.HtmlElements
                array.Add(QueryObject(item, query))
            Next
        End If

        Return array
    End Function

    Private Function QueryObject(document As HtmlElement, query As Query) As JsonObject
        Dim obj As New JsonObject

        For Each member As Query In query.members
            Call obj.Add(member.name, Execute(document, member))
        Next

        Return obj
    End Function
End Class

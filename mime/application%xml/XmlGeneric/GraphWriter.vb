#Region "Microsoft.VisualBasic::05f554580061c41650b17eca2240ed8d, mime\application%xml\XmlGeneric\GraphWriter.vb"

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

    '   Total Lines: 130
    '    Code Lines: 104 (80.00%)
    ' Comment Lines: 2 (1.54%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 24 (18.46%)
    '     File Size: 4.56 KB


    ' Class GraphWriter
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: Load, loadGraphTree, LoadXml
    ' 
    '     Sub: WriteAttributes, WriteValue
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq

Public Class GraphWriter

    ReadOnly graph As SoapGraph

    Sub New(type As Type)
        graph = SoapGraph.GetSchema(type, Serializations.XML)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Load(xml As XmlElement) As Object
        Return loadGraphTree(xml, graph)
    End Function

    Private Shared Sub WriteAttributes(xml As XmlElement, obj As Object, parent As SoapGraph)
        For Each attr In xml.attributes
            Dim attrName = attr.Key.GetTagValue("!")
            Dim key As String = attrName.Value
            Dim value As Object = Nothing

            ' just primitive type or primitive array type
            If parent.writers.ContainsKey(key) Then
                Dim prop = parent.writers(key)
                Dim type As Type = prop.PropertyType

                If type.IsArray Then
                    value = Scripting.CTypeDynamic(attr.Value.Split, type)
                Else
                    value = Scripting.CTypeDynamic(attr.Value, type)
                End If

                Call prop.SetValue(obj, value)
            Else
                Call $"{parent} missing attribute property '{key}'!".Warning
            End If
        Next
    End Sub

    Private Shared Function loadGraphTree(xml As XmlElement, parent As SoapGraph) As Object
        Dim members As New Dictionary(Of String, XmlElement())

        If Not xml.elements.IsNullOrEmpty Then
            members = xml.elements _
               .GroupBy(Function(xi) xi.name) _
               .ToDictionary(Function(xi) xi.Key,
                             Function(xi)
                                 Return xi.ToArray
                             End Function)
        End If

        Dim obj As Object = parent.Activate(
            parent:=parent,
            docs:=members.Keys.ToArray,
            schema:=parent
        )
        Dim text As PropertyInfo = parent.writers.Values _
            .Where(Function(p)
                       Return Not p.GetCustomAttribute(Of XmlTextAttribute) Is Nothing
                   End Function) _
            .FirstOrDefault

        If Not xml.attributes.IsNullOrEmpty Then
            Call WriteAttributes(xml, obj, parent)
        End If

        For Each objVal In members
            If parent.writers.ContainsKey(objVal.Key) Then
                Call WriteValue(
                    objKey:=objVal.Key,
                    obj:=obj,
                    docs:=objVal.Value,
                    define:=parent.writers(objVal.Key)
                )
            Else
#If DEBUG Then
                Call $"missing {xml.name}::{objVal.Key} from schema {parent.ToString}".Warning
#End If
            End If
        Next

        If Not text Is Nothing Then
            Call text.SetValue(obj, Scripting.CTypeDynamic(xml.text, text.PropertyType))
        End If

        Return obj
    End Function

    Private Shared Sub WriteValue(objKey As String, obj As Object, docs As XmlElement(), define As PropertyInfo)
        Dim value As Object

        If Not define.PropertyType.IsArray Then
            If docs.Length > 1 Then
                ' warning
                Call $"{objKey}(array) -> {obj.GetType.FullName}::{define.Name}(scalar) type mis-matched!".Warning
            End If

            value = loadGraphTree(
                xml:=docs(Scan0),
                parent:=SoapGraph.GetSchema(define.PropertyType, Serializations.XML)
            )
        Else
            Dim element As Type = define.PropertyType.GetElementType
            Dim array As Array = Array.CreateInstance(element, docs.Length)
            Dim elementGraph = SoapGraph.GetSchema(element, Serializations.XML)

            For i As Integer = 0 To array.Length - 1
                value = loadGraphTree(docs(i), elementGraph)
                array.SetValue(value, i)
            Next

            value = array
        End If

        Call define.SetValue(obj, value)
    End Sub

    Public Shared Function LoadXml(Of T)(xml As String) As T
        Dim doc As XmlElement = XmlElement.ParseXmlText(xml.SolveStream)
        Dim writer As New GraphWriter(GetType(T))
        Dim obj As Object = writer.Load(doc)

        Return obj
    End Function

End Class

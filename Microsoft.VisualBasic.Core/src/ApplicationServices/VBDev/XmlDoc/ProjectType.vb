#Region "Microsoft.VisualBasic::5e6fb82ef646f86895796c2c71d0f124, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\ProjectType.vb"

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

    '   Total Lines: 187
    '    Code Lines: 129
    ' Comment Lines: 18
    '   Blank Lines: 40
    '     File Size: 7.16 KB


    '     Class ProjectType
    ' 
    '         Properties: [Namespace], Name, Remarks, Summary
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: EnsureEvent, EnsureField, EnsureMethod, EnsureProperty, GetEvent
    '                   GetField, getInternal, GetMethods, GetProperties, ToString
    ' 
    '         Sub: LoadFromNode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development.XmlDoc.Assembly

    Public MustInherit Class XmlDocs

        Public Property Name As String
        Public Property Summary As String
        Public Property Remarks As String

    End Class

    ''' <summary>
    ''' A type within a project namespace.
    ''' </summary>
    ''' <remarks>
    ''' Fields和Events都不允许重载，但是属性和函数都可以重载
    ''' </remarks>
    Public Class ProjectType : Inherits XmlDocs

        Protected projectNamespace As ProjectNamespace

        Protected fields As Dictionary(Of String, ProjectMember)
        Protected events As Dictionary(Of String, ProjectMember)

        ''' <summary>
        ''' 因为属性存在参数，所以可能会出现重载的情况
        ''' </summary>
        Protected Friend properties As Dictionary(Of String, List(Of ProjectMember))
        ''' <summary>
        ''' 会出现重载函数，所以这里也应该是一个list
        ''' </summary>
        Protected Friend methods As Dictionary(Of String, List(Of ProjectMember))

        ''' <summary>
        ''' The namespace container of this type
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [Namespace]() As ProjectNamespace
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.projectNamespace
            End Get
        End Property

        Friend Sub New()
            properties = New Dictionary(Of String, List(Of ProjectMember))
            methods = New Dictionary(Of String, List(Of ProjectMember))
        End Sub

        Public Sub New(projectNamespace As ProjectNamespace)
            Me.projectNamespace = projectNamespace

            Me.fields = New Dictionary(Of String, ProjectMember)()
            Me.properties = New Dictionary(Of String, List(Of ProjectMember))()
            Me.methods = New Dictionary(Of String, List(Of ProjectMember))()
            Me.events = New Dictionary(Of String, ProjectMember)
        End Sub

        Protected Sub New(type As ProjectType)
            projectNamespace = type.projectNamespace
            events = type.events
            fields = type.fields
            properties = type.properties
            methods = type.methods
            Name = type.Name
            Summary = type.Summary
            Remarks = type.Remarks
        End Sub

        Friend Sub New(t1 As ProjectType, t2 As ProjectType)
            projectNamespace = t1.projectNamespace
            fields = (t1.fields.Values.AsList + t2.fields.Values).GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.Sum(Me))
            events = (t1.events.Values.AsList + t2.events.Values).GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.Sum(Me))
            properties = (t1.properties.Values.AsList + t2.properties.Values).IteratesALL.GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.ToList)
            methods = (t1.methods.Values.AsList + t2.methods.Values).IteratesALL.GroupBy(Function(f) f.Name.ToLower).ToDictionary(Function(g) g.Key, Function(g) g.ToList)
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMethods(methodName As String) As List(Of ProjectMember)
            Return getInternal(methods, methodName.ToLower)
        End Function

        Friend Function EnsureMethod(methodName As String) As ProjectMember
            Dim pmlist As List(Of ProjectMember) = Me.GetMethods(methodName)
            Dim pm As New ProjectMember(Me) With {
                .Name = methodName
            }

            Call pmlist.Add(pm)

            Return pm
        End Function

        Private Shared Function getInternal(ByRef table As Dictionary(Of String, List(Of ProjectMember)), name$) As List(Of ProjectMember)
            If table.ContainsKey(name) Then
                Return table(name)
            Else
                Dim list As New List(Of ProjectMember)
                table.Add(name, list)
                Return list
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetProperties(propertyName As String) As List(Of ProjectMember)
            Return getInternal(properties, propertyName.ToLower)
        End Function

        Friend Function EnsureProperty(propertyName As String) As ProjectMember
            Dim pmlist As List(Of ProjectMember) = Me.GetProperties(propertyName)
            Dim pm As New ProjectMember(Me) With {
                .Name = propertyName
            }

            Call pmlist.Add(pm)

            Return pm
        End Function

        Public Function GetField(fieldName As String) As ProjectMember
            If Me.fields.ContainsKey(fieldName.ToLower()) Then
                Return Me.fields(fieldName.ToLower())
            End If

            Return Nothing
        End Function

        Friend Function EnsureField(fieldName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetField(fieldName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me) With {
                    .Name = fieldName
                }

                Me.fields.Add(fieldName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Function GetEvent(eventName As String) As ProjectMember
            If Me.events.ContainsKey(eventName.ToLower()) Then
                Return Me.events(eventName.ToLower())
            End If

            Return Nothing
        End Function

        Friend Function EnsureEvent(eventName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetField(eventName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me) With {
                    .Name = eventName
                }

                Me.fields.Add(eventName.ToLower(), pm)
            End If

            Return pm
        End Function

        Friend Sub LoadFromNode(xn As XmlNode)
            Dim summaryNode As XmlNode = xn.SelectSingleNode("summary")

            If summaryNode IsNot Nothing Then
                Me.Summary = summaryNode.InnerText.Trim(ASCII.CR, ASCII.LF, " ")
            End If

            summaryNode = xn.SelectSingleNode("remarks")

            If Not summaryNode Is Nothing Then
                Remarks = summaryNode.InnerText.Trim(ASCII.CR, ASCII.LF, " ")
            End If
        End Sub
    End Class
End Namespace

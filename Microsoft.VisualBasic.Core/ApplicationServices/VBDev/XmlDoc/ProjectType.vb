#Region "Microsoft.VisualBasic::b1a1ea07fb5ce7820363aefac6bc88f8, ..\sciBASIC#\Microsoft.VisualBasic.Core\ApplicationServices\VBDev\XmlDoc\ProjectType.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Runtime.CompilerServices
Imports System.Xml

Namespace ApplicationServices.Development.XmlDoc.Assembly

    ''' <summary>
    ''' A type within a project namespace.
    ''' </summary>
    Public Class ProjectType

        Protected projectNamespace As ProjectNamespace
        Protected fields As Dictionary(Of String, ProjectMember)
        Protected properties As Dictionary(Of String, ProjectMember)
        Protected methods As Dictionary(Of String, ProjectMember)

        Public ReadOnly Property [Namespace]() As ProjectNamespace
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.projectNamespace
            End Get
        End Property

        Public Property Name As String
        Public Property Summary As String
        Public Property Remarks As String

        Public Sub New(projectNamespace As ProjectNamespace)
            Me.projectNamespace = projectNamespace

            Me.fields = New Dictionary(Of String, ProjectMember)()
            Me.properties = New Dictionary(Of String, ProjectMember)()
            Me.methods = New Dictionary(Of String, ProjectMember)()
        End Sub

        Protected Sub New(type As ProjectType)
            projectNamespace = type.projectNamespace
            fields = type.fields
            properties = type.properties
            methods = type.methods
            Name = type.Name
            Summary = type.Summary
            Remarks = type.Remarks
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Function GetMethod(methodName As String) As ProjectMember
            If Me.methods.ContainsKey(methodName.ToLower()) Then
                Return Me.methods(methodName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureMethod(methodName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetMethod(methodName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me) With {
                    .Name = methodName
                }

                Me.methods.Add(methodName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Function GetProperty(propertyName As String) As ProjectMember
            If Me.properties.ContainsKey(propertyName.ToLower()) Then
                Return Me.properties(propertyName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureProperty(propertyName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetProperty(propertyName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me) With {
                    .Name = propertyName
                }

                Me.properties.Add(propertyName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Function GetField(fieldName As String) As ProjectMember
            If Me.fields.ContainsKey(fieldName.ToLower()) Then
                Return Me.fields(fieldName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureField(fieldName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetField(fieldName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me) With {
                    .Name = fieldName
                }

                Me.fields.Add(fieldName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Sub LoadFromNode(xn As XmlNode)
            Dim summaryNode As XmlNode = xn.SelectSingleNode("summary")

            If summaryNode IsNot Nothing Then
                Me._Summary = summaryNode.InnerText
            End If

            summaryNode = xn.SelectSingleNode("remarks")
            If Not summaryNode Is Nothing Then
                Remarks = summaryNode.InnerText
            End If
        End Sub
    End Class
End Namespace

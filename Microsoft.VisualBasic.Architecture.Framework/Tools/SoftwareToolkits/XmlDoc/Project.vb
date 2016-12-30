#Region "Microsoft.VisualBasic::7ecf22def39e7a3745c86e4755be7cdc, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\Project.vb"

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

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Xml

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' Describes a Project, a collection of related types and namespaces.  In this case, one Project = one DLL.
    ''' </summary>
    Public Class Project

        Private _namespaces As Dictionary(Of String, ProjectNamespace)

        Public Property Name() As String

        Public ReadOnly Property Namespaces() As IEnumerable(Of ProjectNamespace)
            Get
                Return Me._namespaces.Values
            End Get
        End Property

        Public Sub New(name As String)
            Me._Name = name
            Me._namespaces = New Dictionary(Of String, ProjectNamespace)()
        End Sub

        Public Function GetNamespace(namespacePath As String) As ProjectNamespace
            If Me._namespaces.ContainsKey(namespacePath.ToLower()) Then
                Return Me._namespaces(namespacePath.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureNamespace(namespacePath As String) As ProjectNamespace
            Dim pn As ProjectNamespace = GetNamespace(namespacePath)

            If pn Is Nothing Then
                pn = New ProjectNamespace(Me) With {
                    .Path = namespacePath
                }

                _namespaces.Add(namespacePath.ToLower(), pn)
            End If

            Return pn
        End Function

        Friend Sub ProcessXmlDoc(document As XmlDocument)
            Dim memberNodes As XmlNodeList = document.DocumentElement.SelectNodes("members/member")

            For Each memberNode As XmlNode In memberNodes
                Dim memberDescription As String = memberNode.Attributes.GetNamedItem("name").InnerText
                Dim firstSemicolon As Integer = memberDescription.IndexOf(":")

                If firstSemicolon = 1 Then
                    Dim typeChar As Char = memberDescription(0)

                    If typeChar = "T"c Then
                        Dim typeFullName As String = memberDescription.Substring(2, memberDescription.Length - 2)
                        Dim lastPeriod As Integer = typeFullName.LastIndexOf(".")

                        lastPeriod = typeFullName.LastIndexOf(".")

                        If lastPeriod > 0 Then
                            Dim namespaceFullName As String = typeFullName.Substring(0, lastPeriod)

                            Dim typeShortName As String = typeFullName.Substring(lastPeriod + 1, typeFullName.Length - (lastPeriod + 1))

                            Me.EnsureNamespace(namespaceFullName).EnsureType(typeShortName).LoadFromNode(memberNode)
                        End If
                    Else
                        Dim memberFullName As String = memberDescription.Substring(2, memberDescription.Length - 2)

                        Dim firstParen As Integer = memberFullName.IndexOf("(")

                        If firstParen > 0 Then
                            memberFullName = memberFullName.Substring(0, firstParen)
                        End If

                        Dim lastPeriod As Integer = memberFullName.LastIndexOf(".")

                        If lastPeriod > 0 Then
                            Dim typeFullName As String = memberFullName.Substring(0, lastPeriod)

                            lastPeriod = typeFullName.LastIndexOf(".")

                            If lastPeriod > 0 Then
                                Dim namespaceFullName As String = typeFullName.Substring(0, lastPeriod)

                                lastPeriod = typeFullName.LastIndexOf(".")

                                If lastPeriod > 0 Then
                                    Dim typeShortName As String = typeFullName.Substring(lastPeriod + 1, typeFullName.Length - (lastPeriod + 1))


                                    lastPeriod = memberFullName.LastIndexOf(".")

                                    If lastPeriod > 0 Then
                                        Dim memberShortName As String = memberFullName.Substring(lastPeriod + 1, memberFullName.Length - (lastPeriod + 1))

                                        Dim pn As ProjectNamespace = Me.EnsureNamespace(namespaceFullName)

                                        Dim pt As ProjectType = pn.EnsureType(typeShortName)

                                        If typeChar = "M"c Then
                                            pt.EnsureMethod(memberShortName).LoadFromNode(memberNode)
                                        ElseIf typeChar = "P"c OrElse typeChar = "F" Then
                                            pt.EnsureProperty(memberShortName).LoadFromNode(memberNode)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Next
        End Sub
    End Class
End Namespace

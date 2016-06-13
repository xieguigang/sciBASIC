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

        Private m_namespaces As Dictionary(Of String, ProjectNamespace)

        Public Property Name() As String

        Public ReadOnly Property Namespaces() As IEnumerable(Of ProjectNamespace)
            Get
                Return Me.m_namespaces.Values
            End Get
        End Property

        Public Sub New(name As String)
            Me._Name = name
            Me.m_namespaces = New Dictionary(Of String, ProjectNamespace)()
        End Sub

        Public Function GetNamespace(namespacePath As String) As ProjectNamespace
            If Me.m_namespaces.ContainsKey(namespacePath.ToLower()) Then
                Return Me.m_namespaces(namespacePath.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureNamespace(namespacePath As String) As ProjectNamespace
            Dim pn As ProjectNamespace = Me.GetNamespace(namespacePath)

            If pn Is Nothing Then
                pn = New ProjectNamespace(Me)
                pn.Path = namespacePath

                Me.m_namespaces.Add(namespacePath.ToLower(), pn)
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
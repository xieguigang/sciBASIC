#Region "Microsoft.VisualBasic::e35de68a88a7a324448305ba64a66c3f, ..\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\ProjectType.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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
Imports System.IO
Imports System.Xml

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' A type within a project namespace.
    ''' </summary>
    Public Class ProjectType
        Private projectNamespace As ProjectNamespace
        Private fields As Dictionary(Of String, ProjectMember)
        Private properties As Dictionary(Of String, ProjectMember)
        Private methods As Dictionary(Of String, ProjectMember)

        Public ReadOnly Property [Namespace]() As ProjectNamespace
            Get
                Return Me.projectNamespace
            End Get
        End Property

        Public Property Name() As String
        Public Property Summary() As String
        Public Property remarks As String

        Public Sub New(projectNamespace As ProjectNamespace)
            Me.projectNamespace = projectNamespace

            Me.fields = New Dictionary(Of String, ProjectMember)()
            Me.properties = New Dictionary(Of String, ProjectMember)()
            Me.methods = New Dictionary(Of String, ProjectMember)()
        End Sub

        Public Function GetMethod(methodName As String) As ProjectMember
            If Me.methods.ContainsKey(methodName.ToLower()) Then
                Return Me.methods(methodName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureMethod(methodName As String) As ProjectMember
            Dim pm As ProjectMember = Me.GetMethod(methodName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me)
                pm.Name = methodName

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
                pm = New ProjectMember(Me)
                pm.Name = propertyName

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
                pm = New ProjectMember(Me)
                pm.Name = fieldName

                Me.fields.Add(fieldName.ToLower(), pm)
            End If

            Return pm
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="folderPath"></param>
        ''' <param name="pageTemplate"></param>
        ''' <param name="hexoPublish"></param>
        ''' <remarks>这里还应该包括完整的函数的参数注释的输出</remarks>
        Public Sub ExportMarkdownFile(folderPath As String, pageTemplate As String, Optional hexoPublish As Boolean = False)
            Dim methodList As New StringBuilder()

            If Me.methods.Values.Count > 0 Then
                methodList.AppendLine("### Methods" & vbCr & vbLf)

                Dim sortedMembers As SortedList(Of String, ProjectMember) = New SortedList(Of String, ProjectMember)()

                For Each pm As ProjectMember In Me.methods.Values
                    sortedMembers.Add(pm.Name, pm)
                Next

                For Each pm As ProjectMember In sortedMembers.Values
                    methodList.AppendLine("#### " & pm.Name)
                    If Not pm.Declare.IsBlank Then
                        methodList.AppendLine("```csharp")
                        methodList.AppendLine($"{pm.Declare}")
                        methodList.AppendLine("```")
                    End If
                    methodList.AppendLine(CleanText(pm.Summary))

                    If Not pm.param.IsNullOrEmpty Then
                        Call methodList.AppendLine()
                        Call methodList.AppendLine("|Parameter Name|Remarks|")
                        Call methodList.AppendLine("|--------------|-------|")

                        For Each arg In pm.param
                            Call methodList.AppendLine($"|{arg.name}|{arg.text}|")
                        Next

                        Call methodList.AppendLine()
                    End If

                    If Not pm.Returns.IsBlank Then
                        If Not hexoPublish Then
                            methodList.AppendLine()
                        End If
                        methodList.AppendLine("_returns: " & pm.Returns & "_")
                    End If

                    If Not pm.Remarks.IsBlank Then
                        For Each line As String In pm.Remarks.lTokens
                            Call methodList.AppendLine("> " & line)
                        Next
                    End If

                    Call methodList.AppendLine()
                Next
            End If

            Dim propertyList As New StringBuilder()

            If Me.properties.Count > 0 Then
                propertyList.AppendLine("### Properties" & vbCr & vbLf)

                Dim sortedMembers As SortedList(Of String, ProjectMember) = New SortedList(Of String, ProjectMember)()

                For Each pm As ProjectMember In Me.properties.Values
                    sortedMembers.Add(pm.Name, pm)
                Next

                For Each pm As ProjectMember In sortedMembers.Values
                    propertyList.AppendLine("#### " & pm.Name)
                    propertyList.AppendLine(CleanText(pm.Summary))
                Next
            End If

            Dim rmk As String = ""

            For Each l As String In remarks.lTokens
                rmk &= "> " & l & vbCrLf
            Next

            If Trim(rmk) = ">" OrElse rmk.IsBlank Then
                rmk = ""
            End If

            Dim ext As String = If(hexoPublish, ".html", ".md")
            Dim text As String = String.Format("# {0}" & vbCr & vbLf &
                                               "_namespace: [{1}](N-{1}" & $"{ext})_" & vbCr & vbLf &
                                               vbCr & vbLf &
                                               "{2}" & vbCr & vbLf &
                                               vbCr & vbLf &
                                               "{3}" & vbCr & vbLf &
                                               vbCr & vbLf &
                                               "{4}" & vbCr & vbLf &
                                               "{5}", Me.Name, Me.[Namespace].Path, CleanText(Me._Summary), rmk, methodList.ToString(), propertyList.ToString())

            If hexoPublish Then
                text = $"---
title: {Me.Name}
---

" & text
            Else
                If pageTemplate IsNot Nothing Then
                    text = pageTemplate.Replace("[content]", text)
                End If
            End If

            Call text.SaveTo(folderPath & "/T-" & Me.[Namespace].Path & "." & Me.Name & ".md", Encoding.UTF8)
        End Sub

        Public Sub LoadFromNode(xn As XmlNode)
            Dim summaryNode As XmlNode = xn.SelectSingleNode("summary")

            If summaryNode IsNot Nothing Then
                Me._Summary = summaryNode.InnerText
            End If

            summaryNode = xn.SelectSingleNode("remarks")
            If Not summaryNode Is Nothing Then
                remarks = summaryNode.InnerText
            End If
        End Sub


        Private Function CleanText(incomingText As String) As String
            If incomingText Is Nothing Then
                Return String.Empty
            End If

            incomingText = incomingText.Replace(vbTab, "").Trim()

            Dim results As String = String.Empty
            Dim lastCharWasSpace As Boolean = False
            For Each c As Char In incomingText
                If c <> " "c Then
                    lastCharWasSpace = False
                    results += c
                ElseIf Not lastCharWasSpace Then
                    lastCharWasSpace = True
                    results += c
                End If
            Next

            Return results
        End Function
    End Class
End Namespace

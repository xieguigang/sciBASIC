#Region "Microsoft.VisualBasic::8e6c3d13a900910d0f36ecfac82b75d7, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\ProjectNamespace.vb"

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
Imports Microsoft.VisualBasic.SoftwareToolkits.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Text

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' A namespace within a project -- typically a collection of related types.  Equates to a .net Namespace.
    ''' </summary>
    Public Class ProjectNamespace

        Dim project As Project
        Dim _types As Dictionary(Of String, ProjectType)

        Public Property Path() As String

        Public ReadOnly Property Types() As IEnumerable(Of ProjectType)
            Get
                Return Me._types.Values
            End Get
        End Property
        Public Sub New(project As Project)
            Me.project = project
            Me._types = New Dictionary(Of String, ProjectType)()
        End Sub

        Public Overloads Function [GetType](typeName As String) As ProjectType
            If Me._types.ContainsKey(typeName.ToLower()) Then
                Return Me._types(typeName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureType(typeName As String) As ProjectType
            Dim pt As ProjectType = Me.[GetType](typeName)

            If pt Is Nothing Then
                pt = New ProjectType(Me) With {
                    .Name = typeName
                }

                Me._types.Add(typeName.ToLower(), pt)
            End If

            Return pt
        End Function

        ''' <summary>
        ''' Exports for namespace markdown documents
        ''' </summary>
        ''' <param name="folderPath"></param>
        ''' <param name="pageTemplate"></param>
        ''' <param name="url"></param>
        Public Sub ExportMarkdownFile(folderPath As String, pageTemplate As String, url As URLBuilder)
            Dim typeList As New StringBuilder()
            Dim projectTypes As New SortedList(Of String, ProjectType)()

            For Each pt As ProjectType In Me.Types
                projectTypes.Add(pt.Name, pt)
            Next

            Call typeList.AppendLine("|Type|Summary|")
            Call typeList.AppendLine("|----|-------|")

            For Each pt As ProjectType In projectTypes.Values
                Dim lines$() = If(pt.Summary Is Nothing, "", pt.Summary) _
                    .Trim(ASCII.CR, ASCII.LF) _
                    .Trim _
                    .lTokens
                Dim summary$ = If(lines.IsNullOrEmpty OrElse lines.Length = 1,
                    lines.FirstOrDefault,
                    lines.First & " ...")
                Dim link As String = url.GetNamespaceTypeUrl(Me, pt)

                Call typeList.AppendLine($"|{link}|{summary}|")
            Next

            Dim text As String
            Dim path$ = url.GetNamespaceSave(folderPath, Me) ' *.md output path

            If url.[lib] = Libraries.Hexo Then
                text = $"---
title: {Me.Path}
---"
                text = text & vbCrLf & vbCrLf & typeList.ToString
            Else
                text = vbCr & vbLf & "# {0}" & vbCr & vbLf & vbCr & vbLf & "{1}" & vbCr & vbLf
                text = String.Format(text, Me.Path, typeList.ToString())
            End If

            If pageTemplate IsNot Nothing Then
                text = pageTemplate.Replace("[content]", text)
            End If

            Call text.SaveTo(path, Encoding.UTF8)
        End Sub
    End Class
End Namespace

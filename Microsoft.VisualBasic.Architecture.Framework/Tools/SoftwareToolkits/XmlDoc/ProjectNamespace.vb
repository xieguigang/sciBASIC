#Region "Microsoft.VisualBasic::6f41b5359ec501222fb4f049d7d20ecf, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\XmlDoc\ProjectNamespace.vb"

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

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' A namespace within a project -- typically a collection of related types.  Equates to a .net Namespace.
    ''' </summary>
    Public Class ProjectNamespace

        Dim project As Project
        Dim m_types As Dictionary(Of String, ProjectType)

        Public Property Path() As String

        Public ReadOnly Property Types() As IEnumerable(Of ProjectType)
            Get
                Return Me.m_types.Values
            End Get
        End Property
        Public Sub New(project As Project)
            Me.project = project
            Me.m_types = New Dictionary(Of String, ProjectType)()
        End Sub

        Public Overloads Function [GetType](typeName As String) As ProjectType
            If Me.m_types.ContainsKey(typeName.ToLower()) Then
                Return Me.m_types(typeName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureType(typeName As String) As ProjectType
            Dim pt As ProjectType = Me.[GetType](typeName)

            If pt Is Nothing Then
                pt = New ProjectType(Me)
                pt.Name = typeName

                Me.m_types.Add(typeName.ToLower(), pt)
            End If

            Return pt
        End Function

        Public Sub ExportMarkdownFile(folderPath As String, pageTemplate As String, Optional hexoPublish As Boolean = False)
            Dim typeList As New StringBuilder()
            Dim projectTypes As New SortedList(Of String, ProjectType)()
            Dim ext As String = If(hexoPublish, ".html", ".md")

            For Each pt As ProjectType In Me.Types
                projectTypes.Add(pt.Name, pt)
            Next

            For Each pt As ProjectType In projectTypes.Values
                typeList.AppendLine("[" & pt.Name & "](T-" & Me.Path & "." & pt.Name & $"{ext})")
            Next

            Dim text As String

            If hexoPublish Then
                text = $"---
title: {Me.Path}
---"
                text = text & vbCrLf & vbCrLf & typeList.ToString
            Else
                text = String.Format(vbCr & vbLf & "# {0}" & vbCr & vbLf & vbCr & vbLf & "{1}" & vbCr & vbLf, Me.Path, typeList.ToString())
            End If

            If pageTemplate IsNot Nothing Then
                text = pageTemplate.Replace("[content]", text)
            End If

            Call text.SaveTo(folderPath & "/N-" & Me.Path & ".md", Encoding.UTF8)
        End Sub
    End Class
End Namespace

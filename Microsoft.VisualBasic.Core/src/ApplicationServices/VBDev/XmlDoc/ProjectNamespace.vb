#Region "Microsoft.VisualBasic::b5b3b027647d4ae24d667f7ccbde5ac4, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\ProjectNamespace.vb"

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

    '   Total Lines: 65
    '    Code Lines: 44
    ' Comment Lines: 5
    '   Blank Lines: 16
    '     File Size: 2.13 KB


    '     Class ProjectNamespace
    ' 
    '         Properties: fullName, Types
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: [GetType], EnsureType, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Development.XmlDoc.Assembly

    ''' <summary>
    ''' A namespace within a project -- typically a collection of related types.  Equates to a .net Namespace.
    ''' </summary>
    Public Class ProjectNamespace

        Protected project As Project
        Protected _types As Dictionary(Of String, ProjectType)

        Public Property fullName As String

        Public ReadOnly Property Types() As IEnumerable(Of ProjectType)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me._types.Values
            End Get
        End Property

        Public Sub New(project As Project)
            Me.project = project
            Me._types = New Dictionary(Of String, ProjectType)()
        End Sub

        Public Sub New(proj As Project, types As Dictionary(Of String, ProjectType))
            Me.project = proj
            Me._types = types
        End Sub

        Protected Sub New(ns As ProjectNamespace)
            Call Me.New(ns.project, ns._types)
        End Sub

        Public Overrides Function ToString() As String
            Return fullName
        End Function

        Public Overloads Function [GetType](typeName As String) As ProjectType
            If Me._types.ContainsKey(typeName.ToLower()) Then
                Return Me._types(typeName.ToLower())
            End If

            Return Nothing
        End Function

        Friend Function EnsureType(typeName As String) As ProjectType
            Dim pt As ProjectType = Me.[GetType](typeName)

            If pt Is Nothing Then
                pt = New ProjectType(Me) With {
                    .Name = typeName
                }

                Me._types.Add(typeName.ToLower(), pt)
            End If

            Return pt
        End Function
    End Class
End Namespace

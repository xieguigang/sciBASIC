#Region "Microsoft.VisualBasic::9d3b0813e17818d8c4fc111dbb7591bb, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\XmlDoc\Project.vb"

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

    '   Total Lines: 165
    '    Code Lines: 116 (70.30%)
    ' Comment Lines: 15 (9.09%)
    '    - Xml Docs: 53.33%
    ' 
    '   Blank Lines: 34 (20.61%)
    '     File Size: 6.64 KB


    '     Class Project
    ' 
    '         Properties: Name, Namespaces
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [GetType], EnsureNamespace, GetNamespace, ToString
    ' 
    '         Sub: processMember, processType, ProcessXmlDoc
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Runtime.CompilerServices
Imports System.Xml
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports Microsoft.VisualBasic.Text

Namespace ApplicationServices.Development.XmlDoc.Assembly

    ''' <summary>
    ''' Describes a Project, a collection of related types and namespaces.  In this case, one Project = one DLL.
    ''' </summary>
    Public Class Project

        Dim _namespaces As Dictionary(Of String, ProjectNamespace)

        Public Property Name As String

        Public ReadOnly Property Namespaces() As IEnumerable(Of ProjectNamespace)
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me._namespaces.Values
            End Get
        End Property

        Public Sub New(name As String)
            _Name = name.Trim(ASCII.CR, ASCII.LF, " ")
            _namespaces = New Dictionary(Of String, ProjectNamespace)()
        End Sub

        Sub New(name$, namespaces As Dictionary(Of String, ProjectNamespace))
            Me.Name = name
            Me._namespaces = namespaces
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Function GetNamespace(namespacePath As String) As ProjectNamespace
            If _namespaces.ContainsKey(namespacePath.ToLower()) Then
                Return _namespaces(namespacePath.ToLower())
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' 当目标对象没有xml注释的时候，这个函数返回的是空值
        ''' </summary>
        ''' <param name="fullName"></param>
        ''' <returns></returns>
        Public Overloads Function [GetType](fullName As String) As ProjectType
            Dim tokens = fullName.Split("."c)
            Dim typeName As String = tokens.Last
            Dim namespaceRef As String = tokens.Take(tokens.Length - 1).JoinBy(".")
            Dim namespaceDoc As ProjectNamespace = GetNamespace(namespaceRef)

            ' if no xml comment docs, 
            ' then namespaceDoc object will be nothing
            If namespaceDoc Is Nothing Then
                Return Nothing
            Else
                Return namespaceDoc.GetType(typeName)
            End If
        End Function

        Friend Function EnsureNamespace(namespacePath As String) As ProjectNamespace
            Dim pn As ProjectNamespace = GetNamespace(namespacePath)

            If pn Is Nothing Then
                pn = New ProjectNamespace(Me) With {
                    .fullName = namespacePath
                }
                _namespaces.Add(namespacePath.ToLower(), pn)
            End If

            Return pn
        End Function

        Friend Sub ProcessXmlDoc(document As XmlDocument, excludeVBSpecific As Boolean)
            Dim memberNodes As XmlNodeList = document _
                .DocumentElement _
                .SelectNodes("members/member")

            For Each memberNode As XmlNode In memberNodes
                Dim memberDescription As String = memberNode.Attributes.GetNamedItem("name").InnerText
                Dim firstSemicolon As Integer = memberDescription.IndexOf(":")

                If excludeVBSpecific AndAlso memberDescription.IsMyResource Then
                    Continue For
                End If

                If firstSemicolon = 1 Then
                    Dim typeChar As Char = memberDescription(0)

                    If typeChar = "T"c Then
                        Call processType(memberNode, memberDescription)
                    Else
                        Call processMember(memberNode, memberDescription, typeChar)
                    End If
                End If
            Next
        End Sub

        Private Sub processMember(memberNode As XmlNode, memberDescription$, typeChar As Char)
            Dim memberFullName As String = memberDescription.Substring(2, memberDescription.Length - 2)
            Dim firstParen As Integer = memberFullName.IndexOf("(")

            ' the parameter list is not a part of the member short name
            If firstParen > 0 Then
                memberFullName = memberFullName.Substring(0, firstParen)
            End If

            ' Ns.Type.Member  ->  member = last segment, typeFullName = the rest
            Dim lastPeriod As Integer = memberFullName.LastIndexOf(".")

            If lastPeriod <= 0 Then
                Return
            End If

            Dim typeFullName As String = memberFullName.Substring(0, lastPeriod)
            Dim memberShortName As String = memberFullName.Substring(lastPeriod + 1)

            ' typeFullName = Ns.Type  ->  type = last segment, namespace = the rest
            Dim typeTokens = typeFullName.Split("."c)
            Dim typeShortName As String = typeTokens.Last
            Dim namespaceFullName As String = typeTokens.Take(typeTokens.Length - 1).JoinBy(".")

            Dim pn As ProjectNamespace = Me.EnsureNamespace(namespaceFullName)
            Dim pt As ProjectType = pn.EnsureType(typeShortName)

            Select Case typeChar
                Case "M"c
                    pt.EnsureMethod(memberShortName).LoadFromNode(memberNode)
                Case "P"c
                    pt.EnsureProperty(memberShortName).LoadFromNode(memberNode)
                Case "F"c
                    pt.EnsureField(memberShortName).LoadFromNode(memberNode)
                Case "E"c
                    pt.EnsureEvent(memberShortName).LoadFromNode(memberNode)
                Case Else
                    Throw New NotImplementedException
            End Select
        End Sub

        Private Sub processType(memberNode As XmlNode, memberDescription$)
            Dim typeFullName As String = memberDescription.Substring(2, memberDescription.Length - 2)
            Dim tokens = typeFullName.Split("."c)

            If tokens.Length < 1 Then
                Return
            End If

            Dim typeShortName As String = tokens.Last
            Dim namespaceFullName As String = tokens.Take(tokens.Length - 1).JoinBy(".")

            Call EnsureNamespace(namespaceFullName) _
                .EnsureType(typeShortName) _
                .LoadFromNode(memberNode)
        End Sub
    End Class
End Namespace

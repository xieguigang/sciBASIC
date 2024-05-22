#Region "Microsoft.VisualBasic::e8486a30c239586537e6dea3be6d41f2, mime\text%yaml\1.1\Base\YAMLSequenceNode.vb"

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

    '   Total Lines: 182
    '    Code Lines: 151 (82.97%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 31 (17.03%)
    '     File Size: 5.63 KB


    '     Class YAMLSequenceNode
    ' 
    '         Properties: IsIndent, IsMultyline, NodeType, Style
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: (+12 Overloads) Add, Emit, EndChild, EndChildren, StartChild
    '              StartChildren
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic

Namespace Grammar11

    Public NotInheritable Class YAMLSequenceNode
        Inherits YAMLNode

        ReadOnly m_children As New List(Of YAMLNode)()

        Public Sub New()
        End Sub

        Public Sub New(style As SequenceStyle)
            Me.Style = style
        End Sub

        Public Sub Add(value As Boolean)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As Byte)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As Short)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As UShort)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As Integer)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As UInteger)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As Long)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As ULong)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As Single)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As Double)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(value As String)
            Dim node As New YAMLScalarNode(value)
            node.Style = Style.ToScalarStyle()
            Add(node)
        End Sub

        Public Sub Add(child As YAMLNode)
            m_children.Add(child)
        End Sub

        Friend Overrides Sub Emit(emitter As Emitter)
            MyBase.Emit(emitter)

            StartChildren(emitter)
            For Each child As YAMLNode In m_children
                StartChild(emitter, child)
                child.Emit(emitter)
                EndChild(emitter, child)
            Next
            EndChildren(emitter)
        End Sub

        Private Sub StartChildren(emitter As Emitter)
            If Style = SequenceStyle.Block Then
                If m_children.Count = 0 Then
                    emitter.Write("["c)
                End If
            ElseIf Style = SequenceStyle.Flow Then
                emitter.Write("["c)
            ElseIf Style = SequenceStyle.Raw Then
                If m_children.Count = 0 Then
                    emitter.Write("["c)
                End If
            End If
        End Sub

        Private Sub EndChildren(emitter As Emitter)
            If Style = SequenceStyle.Block Then
                If m_children.Count = 0 Then
                    emitter.Write("]"c)
                End If
                emitter.WriteLine()
            ElseIf Style = SequenceStyle.Flow Then
                emitter.WriteClose("]"c)
            ElseIf Style = SequenceStyle.Raw Then
                If m_children.Count = 0 Then
                    emitter.Write("]"c)
                End If
                emitter.WriteLine()
            End If
        End Sub

        Private Sub StartChild(emitter As Emitter, [next] As YAMLNode)
            If Style = SequenceStyle.Block Then
                emitter.IncreaseIntent()
                emitter.Write("-"c).WriteWhitespace()

                If [next].NodeType = NodeType Then
                    emitter.IncreaseIntent()
                End If
            End If
            If [next].IsIndent Then
                emitter.IncreaseIntent()
            End If
        End Sub

        Private Sub EndChild(emitter As Emitter, [next] As YAMLNode)
            If Style = SequenceStyle.Block Then
                emitter.WriteLine()
                emitter.DecreaseIntent()

                If [next].NodeType = NodeType Then
                    emitter.DecreaseIntent()
                End If
            ElseIf Style = SequenceStyle.Flow Then
                emitter.WriteSeparator().WriteWhitespace()
            End If
            If [next].IsIndent Then
                emitter.DecreaseIntent()
            End If
        End Sub

        Public Shared Empty As New YAMLSequenceNode()

        Public Overrides ReadOnly Property NodeType() As YAMLNodeType
            Get
                Return YAMLNodeType.Sequence
            End Get
        End Property

        Public Overrides ReadOnly Property IsMultyline() As Boolean
            Get
                Return Style = SequenceStyle.Block AndAlso m_children.Count > 0
            End Get
        End Property

        Public Overrides ReadOnly Property IsIndent() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Property Style As SequenceStyle

    End Class
End Namespace

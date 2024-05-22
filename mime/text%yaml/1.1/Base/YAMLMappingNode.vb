#Region "Microsoft.VisualBasic::ca108dd184282e0e0e2b9ed250f67b64, mime\text%yaml\1.1\Base\YAMLMappingNode.vb"

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

    '   Total Lines: 244
    '    Code Lines: 198 (81.15%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 46 (18.85%)
    '     File Size: 8.06 KB


    '     Class YAMLMappingNode
    ' 
    '         Properties: IsIndent, IsMultyline, NodeType, Style
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: (+22 Overloads) Add, Concatenate, Emit, EndChildren, EndTransition
    '              (+3 Overloads) InsertBegin, InsertEnd, StartChildren, StartTransition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Grammar11

    Public NotInheritable Class YAMLMappingNode
        Inherits YAMLNode

        ReadOnly m_children As New List(Of KeyValuePair(Of YAMLNode, YAMLNode))()

        Public Sub New()
        End Sub

        Public Sub New(style As MappingStyle)
            Me.Style = style
        End Sub

        Public Sub Add(key As String, value As Boolean)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As Byte)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As Short)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As UShort)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As Integer)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As UInteger)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As Long)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As ULong)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As Single)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As String)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As String, value As YAMLNode)
            Dim keyNode As New YAMLScalarNode(key)
            InsertEnd(keyNode, value)
        End Sub

        Public Sub Add(key As YAMLNode, value As Boolean)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As Byte)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As Short)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As UShort)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As Integer)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As UInteger)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As Long)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As ULong)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As Single)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As String)
            Dim valueNode As New YAMLScalarNode(value)
            Add(key, valueNode)
        End Sub

        Public Sub Add(key As YAMLNode, value As YAMLNode)
            If key.NodeType <> YAMLNodeType.Scalar Then
                Throw New Exception($"Only {YAMLNodeType.Scalar} node as a key supported")
            End If

            InsertEnd(key, value)
        End Sub

        Public Sub Concatenate(map As YAMLMappingNode)
            For Each child As KeyValuePair(Of YAMLNode, YAMLNode) In map.m_children
                Add(child.Key, child.Value)
            Next
        End Sub

        Public Sub InsertBegin(key As String, value As Integer)
            Dim valueNode As New YAMLScalarNode(value)
            InsertBegin(key, valueNode)
        End Sub

        Public Sub InsertBegin(key As String, value As YAMLNode)
            Dim keyNode As New YAMLScalarNode(key)
            InsertBegin(keyNode, value)
        End Sub

        Public Sub InsertBegin(key As YAMLNode, value As YAMLNode)
            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value))
            End If
            Dim pair As New KeyValuePair(Of YAMLNode, YAMLNode)(key, value)
            m_children.Insert(0, pair)
        End Sub

        Friend Overrides Sub Emit(emitter As Emitter)
            MyBase.Emit(emitter)

            StartChildren(emitter)

            For Each kvp In m_children
                Dim key As YAMLNode = kvp.Key
                Dim value As YAMLNode = kvp.Value

                key.Emit(emitter)
                StartTransition(emitter, value)
                value.Emit(emitter)
                EndTransition(emitter, value)
            Next

            EndChildren(emitter)
        End Sub

        Private Sub StartChildren(emitter As Emitter)
            If Style = MappingStyle.Block Then
                If m_children.Count = 0 Then
                    emitter.Write("{"c)
                End If
            ElseIf Style = MappingStyle.Flow Then
                emitter.Write("{"c)
            End If
        End Sub

        Private Sub EndChildren(emitter As Emitter)
            If Style = MappingStyle.Block Then
                If m_children.Count = 0 Then
                    emitter.Write("}"c)
                End If
                emitter.WriteLine()
            ElseIf Style = MappingStyle.Flow Then
                emitter.WriteClose("}"c)
            End If
        End Sub

        Private Sub StartTransition(emitter As Emitter, [next] As YAMLNode)
            emitter.Write(":"c).WriteWhitespace()
            If Style = MappingStyle.Block Then
                If [next].IsMultyline Then
                    emitter.WriteLine()
                End If
            End If
            If [next].IsIndent Then
                emitter.IncreaseIntent()
            End If
        End Sub

        Private Sub EndTransition(emitter As Emitter, [next] As YAMLNode)
            If Style = MappingStyle.Block Then
                emitter.WriteLine()
            ElseIf Style = MappingStyle.Flow Then
                emitter.WriteSeparator().WriteWhitespace()
            End If
            If [next].IsIndent Then
                emitter.DecreaseIntent()
            End If
        End Sub

        Private Sub InsertEnd(key As YAMLNode, value As YAMLNode)
            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value))
            End If
            Dim pair As New KeyValuePair(Of YAMLNode, YAMLNode)(key, value)
            m_children.Add(pair)
        End Sub

        Public Shared Empty As New YAMLMappingNode(MappingStyle.Flow)

        Public Overrides ReadOnly Property NodeType() As YAMLNodeType
            Get
                Return YAMLNodeType.Mapping
            End Get
        End Property

        Public Overrides ReadOnly Property IsMultyline() As Boolean
            Get
                Return Style = MappingStyle.Block
            End Get
        End Property

        Public Overrides ReadOnly Property IsIndent() As Boolean
            Get
                Return Style = MappingStyle.Block
            End Get
        End Property

        Public Property Style() As MappingStyle
    End Class
End Namespace

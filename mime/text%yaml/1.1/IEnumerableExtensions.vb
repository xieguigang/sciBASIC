#Region "Microsoft.VisualBasic::7d3c20f90bc15443addf0b9e957045d6, mime\text%yaml\1.1\IEnumerableExtensions.vb"

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

    '   Total Lines: 180
    '    Code Lines: 159
    ' Comment Lines: 0
    '   Blank Lines: 21
    '     File Size: 6.33 KB


    '     Module IEnumerableExtensions
    ' 
    '         Function: (+11 Overloads) ExportYAML
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CompilerServices

Namespace Grammar11

    <StandardModule> <Extension> Public Module IEnumerableExtensions

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of Boolean)) As YAMLNode
            Static sb As New StringBuilder()

            For Each value As Boolean In this
                Dim bvalue As Byte = CByte(If(value, 1, 0))
                sb.Append(bvalue.ToHexString())
            Next
            Dim node As New YAMLScalarNode(sb.ToString())
            sb.Length = 0
            Return node
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of Byte)) As YAMLNode
            Static s_sb As New StringBuilder

            For Each value As Byte In this
                s_sb.Append(value.ToHexString())
            Next
            Dim node As New YAMLScalarNode(s_sb.ToString())
            s_sb.Length = 0
            Return node
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of UShort), isRaw As Boolean) As YAMLNode
            Static s_sb As New StringBuilder

            If isRaw Then
                For Each value As UShort In this
                    s_sb.Append(value.ToHexString())
                Next
                Dim node As New YAMLScalarNode(s_sb.ToString())
                s_sb.Length = 0
                Return node
            Else
                Dim node As New YAMLSequenceNode(SequenceStyle.Block)
                For Each value As UShort In this
                    node.Add(value)
                Next
                Return node
            End If
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of Short), isRaw As Boolean) As YAMLNode
            Static s_sb As New StringBuilder

            If isRaw Then
                For Each value As Short In this
                    s_sb.Append(value.ToHexString())
                Next
                Dim node As New YAMLScalarNode(s_sb.ToString())
                s_sb.Length = 0
                Return node
            Else
                Dim node As New YAMLSequenceNode(SequenceStyle.Block)
                For Each value As Short In this
                    node.Add(value)
                Next
                Return node
            End If
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of UInteger), isRaw As Boolean) As YAMLNode
            Static s_sb As New StringBuilder

            If isRaw Then
                For Each value As UInteger In this
                    s_sb.Append(value.ToHexString())
                Next
                Dim node As New YAMLScalarNode(s_sb.ToString())
                s_sb.Length = 0
                Return node
            Else
                Dim node As New YAMLSequenceNode(SequenceStyle.Block)
                For Each value As UInteger In this
                    node.Add(value)
                Next
                Return node
            End If
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of Integer), isRaw As Boolean) As YAMLNode
            Static s_sb As New StringBuilder

            If isRaw Then
                For Each value As Integer In this
                    s_sb.Append(value.ToHexString())
                Next
                Dim node As New YAMLScalarNode(s_sb.ToString())
                s_sb.Length = 0
                Return node
            Else
                Dim node As New YAMLSequenceNode(SequenceStyle.Block)
                For Each value As Integer In this
                    node.Add(value)
                Next
                Return node
            End If
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of ULong), isRaw As Boolean) As YAMLNode
            Static s_sb As New StringBuilder

            If isRaw Then
                For Each value As ULong In this
                    s_sb.Append(value.ToHexString())
                Next
                Dim node As New YAMLScalarNode(s_sb.ToString())
                s_sb.Length = 0
                Return node
            Else
                Dim node As New YAMLSequenceNode(SequenceStyle.Block)
                For Each value As ULong In this
                    node.Add(value)
                Next
                Return node
            End If
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of Long), isRaw As Boolean) As YAMLNode
            Static s_sb As New StringBuilder

            If isRaw Then
                For Each value As Long In this
                    s_sb.Append(value.ToHexString())
                Next
                Dim node As New YAMLScalarNode(s_sb.ToString())
                s_sb.Length = 0
                Return node
            Else
                Dim node As New YAMLSequenceNode(SequenceStyle.Block)
                For Each value As Long In this
                    node.Add(value)
                Next
                Return node
            End If
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of Single)) As YAMLNode
            Dim node As New YAMLSequenceNode(SequenceStyle.Block)
            For Each value As Single In this
                node.Add(value)
            Next
            Return node
        End Function

        <Extension>
        Public Function ExportYAML(this As IEnumerable(Of Double)) As YAMLNode
            Dim node As New YAMLSequenceNode(SequenceStyle.Block)
            For Each value As Double In this
                node.Add(value)
            Next
            Return node
        End Function

        <Extension> Public Function ExportYAML(this As IEnumerable(Of String)) As YAMLNode
            Dim node As New YAMLSequenceNode(SequenceStyle.Block)
            For Each value As String In this
                node.Add(value)
            Next
            Return node
        End Function
    End Module
End Namespace

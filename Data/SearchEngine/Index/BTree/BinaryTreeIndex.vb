#Region "Microsoft.VisualBasic::bffb2faa3638e71df376c0028bee4b3d, sciBASIC#\Data\SearchEngine\Index\BTree\BinaryTreeIndex.vb"

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

    '   Total Lines: 32
    '    Code Lines: 22
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 902.00 B


    ' Class BinaryTreeIndex
    ' 
    '     Properties: Additionals, Key, Left, My, Right
    '                 Value
    ' 
    '     Function: ToString
    ' 
    '     Sub: Assign
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel

''' <summary>
''' File save model for binary tree
''' </summary>
Public Class BinaryTreeIndex(Of K, V) : Implements IAddress(Of Integer)

    Public Property Key As K
    Public Property Value As V

    <XmlElement>
    Public Property Additionals As V()

    <XmlAttribute>
    Public Property Left As Integer
    <XmlAttribute>
    Public Property Right As Integer
    <XmlAttribute>
    Public Property My As Integer Implements IAddress(Of Integer).Address

    Private Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
        Me.My = address
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return Scripting.ToString(Key)
    End Function

End Class

#Region "Microsoft.VisualBasic::a50d44173bb943ce6ecbba6e9833f4c0, ..\sciBASIC#\Data\BinaryData\BinaryData\Repository\BinarySearchIndex.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Imports System.IO

''' <summary>
''' Using the binary tree as the search index.
''' (将二叉树序列化为二进制文件作为索引文件)
''' </summary>
Public Class BinarySearchIndex

End Class

''' <summary>
''' The node of the binary search tree
''' </summary>
Public Class Index

    ''' <summary>
    ''' 索引关键词
    ''' </summary>
    ''' <returns></returns>
    Public Property Key As String
    ''' <summary>
    ''' 在数据文件之中的偏移量
    ''' </summary>
    ''' <returns></returns>
    Public Property Offset As Long
    Public Property left As Long
    Public Property right As Long

    Public Function Write(out As BinaryDataWriter) As Long
        Call out.Write(Key, BinaryStringFormat.DwordLengthPrefix)
        Call out.Write(Offset)
        Call out.Write(left)
        Call out.Write(right)

        Return out.Position
    End Function
End Class

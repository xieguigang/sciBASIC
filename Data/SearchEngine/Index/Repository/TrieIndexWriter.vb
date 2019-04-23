#Region "Microsoft.VisualBasic::eff06b7025b8c1c0e14304c5078d4d17, Data\BinaryData\BinaryData\Repository\BinarySearchIndex.vb"

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

' Class BinarySearchIndex
' 
' 
' 
' Class Index
' 
'     Properties: Key, left, Offset, right
' 
'     Function: Write
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Text.Parser

''' <summary>
''' 主要是针对于字符串类型的索引文件的构建, 在这里尝试使用字典树来节省存储空间
''' 只支持ASCII字符
''' </summary>
Public Class TrieIndexWriter : Implements IDisposable

    ReadOnly index As BinaryDataWriter
    ReadOnly reader As TrieIndexReader
    ReadOnly root As Long

    Dim length As Long

    Public Const Magic$ = "TrieIndex"

    ''' <summary>
    ''' data pointer(8 bytes int64) with all ASCII printable characters with a ZERO terminated mark
    ''' </summary>
    Public Const allocateSize As Integer = 8 + (95 + 1) * 4

    Sub New(IOdev As Stream)
        index = New BinaryDataWriter(IOdev, encoding:=Encoding.ASCII)
        ' write magic with 9 bytes
        index.Write(Magic, BinaryStringFormat.NoPrefixOrTermination)
        ' no data was associated with root node. 
        ' write a random int64 number with 8 bytes
        index.Write(Now.ToBinary)
        index.Seek(allocateSize, SeekOrigin.Current)

        reader = New TrieIndexReader(IOdev)

        ' all of the term search starts from here
        root = Magic.Length
        reader.Seek(root, SeekOrigin.Begin)
        ' initialize length
        length = Magic.Length + allocateSize

        Call index.Flush()
    End Sub

    ''' <summary>
    ''' Only supports ASCII symbols
    ''' </summary>
    ''' <param name="term"></param>
    ''' <param name="data">
    ''' 与当前的term所关联的数据块的位置值
    ''' </param>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddTerm(term As String, data As Long)
        Dim offset As Integer
        Dim current As Long

        If Strings.Len(term) = 0 Then
            ' empty string data
            Return
        Else
            ' read from the begining
            Call reader.Seek(root, SeekOrigin.Begin)
        End If

        For Each c As Integer In term.Select(AddressOf Asc)
            ' current is the begining location of current character block
            current = reader.Position
            offset = reader.getNextOffset(c)

            If offset = -1 Then
                ' character c is not exists in current tree routine
                Dim blocks As Integer = (length - current) / allocateSize
                ' write next offset 
                ' from the begining of current block
                ' and then skip the data section
                ' and then jump to the character location
                index.Seek(current + 8 + (c - TrieIndexReader.base) * 4, SeekOrigin.Begin)
                ' write offset block count value
                index.Write(blocks)

                ' jump to location
                index.Position = length
                ' write pre-allocated block
                index.Seek(allocateSize, SeekOrigin.Current)

                current = index.Position
                length += allocateSize
            Else
                Call reader.Seek(current, SeekOrigin.Begin)
                Call reader.Seek(offset, SeekOrigin.Current)
            End If
        Next

        ' End of the charaters is the data entry that associated with current term
        index.Seek(-allocateSize, SeekOrigin.Current)
        index.Write(data)
        ' fill current block with zero data
        ' index.Seek(allocateSize - 8, SeekOrigin.Current)
        index.Flush()
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call index.Flush()
                Call index.Close()
                Call index.Dispose()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class

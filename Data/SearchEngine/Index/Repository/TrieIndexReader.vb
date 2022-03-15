#Region "Microsoft.VisualBasic::50dc22b947bb8c7935e09aac9aa732f5, sciBASIC#\Data\SearchEngine\Index\Repository\TrieIndexReader.vb"

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

    '   Total Lines: 150
    '    Code Lines: 77
    ' Comment Lines: 45
    '   Blank Lines: 28
    '     File Size: 4.64 KB


    ' Class TrieIndexReader
    ' 
    '     Properties: Position
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: CharCode, GetData, getNextOffset, ToString
    ' 
    '     Sub: (+2 Overloads) Dispose, Seek
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text

Public Class TrieIndexReader : Implements IDisposable

    Dim reader As BinaryDataReader
    Dim root As Long = TrieIndexWriter.Magic.Length

    Friend ReadOnly Property Position As Long
        Get
            Return reader.Position
        End Get
    End Property

    Sub New(index As String)
        Call Me.New(index.Open(FileMode.Open, doClear:=False))
    End Sub

    Sub New(index As Stream)
        reader = New BinaryDataReader(index, Encoding.ASCII, leaveOpen:=True)
    End Sub

    Friend Sub Seek(offset As Integer, origin As SeekOrigin)
        Call reader.Seek(offset, origin)
    End Sub

    ''' <summary>
    ''' 获取得到term所关联的数据块的位置, -1结果值表示没有相关的数据
    ''' </summary>
    ''' <param name="term"></param>
    ''' <returns></returns>
    Public Function GetData(term As String) As Long
        Dim offset As Integer
        Dim data As Long
        Dim current As Long

        Call Seek(root, SeekOrigin.Begin)

        For Each c As Integer In term.Select(AddressOf CharCode)
            current = reader.Position
            offset = getNextOffset(c)

            If offset = -1 Then
                Return -1
            Else
                Call Seek(current + offset, SeekOrigin.Begin)
            End If
        Next

        ' End of the charaters is the data entry that associated 
        ' with current term
        ' reader.Seek(-TrieIndexWriter.allocateSize, SeekOrigin.Current)
        data = reader.ReadInt64

        Return data
    End Function

    Const max% = Asc("~"c)

    ''' <summary>
    ''' 在这里将非ASCII的字符都移动到了最后一个字符
    ''' </summary>
    ''' <param name="c"></param>
    ''' <returns></returns>
    Public Shared Function CharCode(c As Char) As Integer
        If c >= " "c AndAlso c <= "~"c Then
            Return Asc(c)
        Else
            Return max + 1
        End If
    End Function

    ' offset block is pre-allocated block
    ' length is (126-32) * 4 + 4 bytes

    ' block_jump offset offset offset offset offset ZERO
    ' 1. offset is the block count for next char
    ' 2. block_jump is the data location that associated with current term string.

    ''' <summary>
    ''' 32
    ''' </summary>
    Public Const base As Integer = Asc(" "c)

    ''' <summary>
    ''' 这个函数是以当前的位置为参考的
    ''' </summary>
    ''' <param name="code"></param>
    ''' <returns></returns>
    Friend Function getNextOffset(code As Integer) As Integer
        ' character block counts
        Dim offset As Integer

        ' skip data section
        reader.Seek(8, SeekOrigin.Current)
        ' jump to character
        reader.Seek((code - base) * 4, SeekOrigin.Current)

        If reader.EndOfStream Then
            ' 已经超过文件的长度了
            ' 说明不存在
            Return -1
        End If

        offset = reader.ReadInt32

        If offset = 0 Then
            Return -1
        Else
            Return offset * TrieIndexWriter.allocateSize
        End If
    End Function

    Public Overrides Function ToString() As String
        Return reader.ToString
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Call reader.Dispose()
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

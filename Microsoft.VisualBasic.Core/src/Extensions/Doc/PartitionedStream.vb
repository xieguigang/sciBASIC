#Region "Microsoft.VisualBasic::349dba59af30a71bbdc1a237da4f671c, Microsoft.VisualBasic.Core\src\Extensions\Doc\PartitionedStream.vb"

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

    '   Total Lines: 116
    '    Code Lines: 70
    ' Comment Lines: 23
    '   Blank Lines: 23
    '     File Size: 4.26 KB


    ' Delegate Function
    ' 
    ' 
    '     Class PartitionedStream
    ' 
    '         Properties: Current, EOF, Total
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: PartitionByLines, ReadPartition, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.Linq

Public Delegate Function PartitioningMethod(block As String, ByRef Left As String) As String()

Namespace Text

    ''' <summary>
    ''' 只是针对文本文件的
    ''' </summary>
    Public Class PartitionedStream : Implements System.IDisposable

        ReadOnly _readerStream As IO.FileStream
        ReadOnly _blockSize As Integer
        ReadOnly _partitions As PartitioningMethod
        ReadOnly _encoding As Encoding

        Sub New(path As String, blockSize As Integer, partitioning As PartitioningMethod, Optional encoding As Encoding = Nothing)
            _readerStream = New IO.FileStream(path, IO.FileMode.Open)
            _blockSize = blockSize
            _partitions = partitioning
            _Total = _readerStream.Length

            If _encoding Is Nothing Then
                _encoding = System.Text.Encoding.Default
            Else
                _encoding = encoding
            End If
        End Sub

        ''' <summary>
        ''' 依照换行符来进行分区
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="blockSize"></param>
        ''' <param name="encoding"></param>
        Sub New(path As String, blockSize As Integer, Optional encoding As Encoding = Nothing)
            Call Me.New(path, blockSize, AddressOf PartitionedStream.PartitionByLines, encoding)
        End Sub

        Public ReadOnly Property EOF As Boolean
            Get
                Return _Current >= _Total
            End Get
        End Property

        Public ReadOnly Property Current As Long
        Public ReadOnly Property Total As Long

        Dim previous As Byte()

        Public Function ReadPartition() As String()
            If EOF Then Return Nothing

            Dim chunkBuffer As Byte()

            If _Current + _blockSize > _Total Then
                chunkBuffer = New Byte(_blockSize - 1) {}
            Else
                chunkBuffer = New Byte(_Total - _Current - 1) {}
            End If

            Call _readerStream.Read(chunkBuffer, _Current, chunkBuffer.Length)
            Call previous.Add(chunkBuffer)

            Dim Text As String = _encoding.GetString(previous)
            Dim rtvl = Me._partitions(Text, Text)
            previous = _encoding.GetBytes(Text)
            Return rtvl
        End Function

        Public Overrides Function ToString() As String
            Return _readerStream.Name
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
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

        Public Shared Function PartitionByLines(block As String, ByRef Left As String) As String()
            Dim tokens As String() = block.LineTokens

            Left = tokens.LastOrDefault
            tokens = tokens.Takes(tokens.Length - 1).ToArray
            Return tokens
        End Function
    End Class
End Namespace

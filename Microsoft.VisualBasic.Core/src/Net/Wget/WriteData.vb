#Region "Microsoft.VisualBasic::8ab80dfb78fe099ccc4ae3378c7bf899, Microsoft.VisualBasic.Core\src\Net\HTTP\Wget\WriteData.vb"

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

    '   Total Lines: 89
    '    Code Lines: 63 (70.79%)
    ' Comment Lines: 11 (12.36%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (16.85%)
    '     File Size: 2.77 KB


    '     Class WriteData
    ' 
    '         Properties: Length
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, Flush, Write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Parallel

Namespace Net.WebClient

    Public Class WriteData : Implements IDisposable

        Private disposedValue As Boolean

        ReadOnly fs As Stream
        ReadOnly pipeline As DuplexPipe

        Public ReadOnly Property Length As Long
            Get
                If Not fs Is Nothing Then
                    Return fs.Length
                Else
                    Return pipeline.Length
                End If
            End Get
        End Property

        Sub New(fs As Stream)
            Me.fs = fs
        End Sub

        Sub New(pipe As DuplexPipe)
            Me.pipeline = pipe
        End Sub

        Public Sub Write(bytes As Byte())
            If Not fs Is Nothing Then
                Call fs.Write(bytes, Scan0, bytes.Length)
            Else
                Call pipeline.Write(bytes)
            End If
        End Sub

        Public Sub Flush()
            If Not fs Is Nothing Then
                Call fs.Flush()
            Else
                ' do nothing
            End If
        End Sub

        Public Overrides Function ToString() As String
            If Not fs Is Nothing Then
                Return "<stream>"
            Else
                Return "<pipeline>"
            End If
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    If Not fs Is Nothing Then
                        Call fs.Flush()
                        Call fs.Close()
                        Call fs.Dispose()
                    Else
                        Call pipeline.Wait()
                        Call pipeline.Close()
                    End If
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class

End Namespace

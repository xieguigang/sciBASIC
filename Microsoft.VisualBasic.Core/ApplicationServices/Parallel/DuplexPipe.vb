#Region "Microsoft.VisualBasic::1566ff5b9f0abffc84056b6a5f291cfd, Microsoft.VisualBasic.Core\ApplicationServices\Parallel\DuplexPipe.vb"

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

    '     Class DuplexPipe
    ' 
    '         Properties: Length
    ' 
    '         Function: GetBlocks, Read
    ' 
    '         Sub: Close, Wait, Write
    ' 
    '     Class BufferPipe
    ' 
    ' 
    ' 
    '     Class DataPipe
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetBlocks, Read
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.Language

Namespace Parallel

    Public Class DuplexPipe : Inherits BufferPipe

        ReadOnly dataFragments As New Queue(Of Byte())
        ReadOnly writeClose As New Value(Of Boolean)(False)

        Public ReadOnly Property Length As Long

        Public Sub Close()
            writeClose.Value = True
        End Sub

        Public Sub Write(buffer As Byte())
            SyncLock dataFragments
                _Length += buffer.Length
                dataFragments.Enqueue(buffer)
            End SyncLock
        End Sub

        Public Sub Wait()
            Do While dataFragments.Count > 0
                Call Thread.Sleep(1)
            Loop
        End Sub

        Public Overrides Function Read() As Byte()
            Do While dataFragments.Count = 0
                If writeClose.Value AndAlso dataFragments.Count = 0 Then
                    Return {}
                Else
                    Call Thread.Sleep(1)
                End If
            Loop

            SyncLock dataFragments
                Return dataFragments.Dequeue
            End SyncLock
        End Function

        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Do While Not writeClose.Value
                Yield Read()
            Loop
        End Function
    End Class

    Public MustInherit Class BufferPipe

        Public MustOverride Iterator Function GetBlocks() As IEnumerable(Of Byte())
        Public MustOverride Function Read() As Byte()

    End Class

    Public Class DataPipe : Inherits BufferPipe

        ReadOnly data As Byte()

        Sub New(data As IEnumerable(Of Byte))
            Me.data = data.ToArray
        End Sub

        Sub New(data As RequestStream)
            Call Me.New(data.Serialize)
        End Sub

        Public Overrides Iterator Function GetBlocks() As IEnumerable(Of Byte())
            Yield data
        End Function

        Public Overrides Function Read() As Byte()
            Return data
        End Function
    End Class
End Namespace

#Region "Microsoft.VisualBasic::da0bda44a53590ae9578890df84047ee, Microsoft.VisualBasic.Core\src\Net\HTTP\Wget\WebClient.vb"

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

    '   Total Lines: 46
    '    Code Lines: 29 (63.04%)
    ' Comment Lines: 4 (8.70%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (28.26%)
    '     File Size: 1.44 KB


    '     Class WebClient
    ' 
    '         Sub: ProgressFinished, ProgressUpdate
    ' 
    '     Class ProgressChangedEventArgs
    ' 
    '         Properties: BytesReceived, ProgressPercentage, TotalBytes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports System.IO

Namespace Net.WebClient

    Public MustInherit Class WebClient

        Public Event DownloadProgressChanged As EventHandler(Of ProgressChangedEventArgs)
        Public Event DownloadCompleted As EventHandler

        Public MustOverride ReadOnly Property LocalSaveFile As String
        Public MustOverride Async Function DownloadFileAsync() As Task

        ''' <summary>
        ''' open save stream
        ''' </summary>
        ''' <returns></returns>
        Protected MustOverride Function OpenSaveStream() As Stream

        Protected Sub ProgressUpdate(args As ProgressChangedEventArgs)
            RaiseEvent DownloadProgressChanged(Me, args)
        End Sub

        Protected Sub ProgressFinished()
            RaiseEvent DownloadCompleted(Me, EventArgs.Empty)
        End Sub

    End Class

    Public Class ProgressChangedEventArgs
        Inherits EventArgs

        Public ReadOnly Property ProgressPercentage As Integer
        Public ReadOnly Property BytesReceived As Long
        Public ReadOnly Property TotalBytes As Long

        Friend Sub New(bytesReceived As Long, totalBytes As Long)
            Me.TotalBytes = totalBytes
            Me.BytesReceived = bytesReceived

            If totalBytes > 0 Then
                Me.ProgressPercentage = CInt((bytesReceived * 100) / totalBytes)
            End If
        End Sub
    End Class
End Namespace

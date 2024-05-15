#Region "Microsoft.VisualBasic::fcef67d082278c0a628092039df11539, www\Microsoft.VisualBasic.NETProtocol\Pipeline\PipeStream.vb"

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

    '   Total Lines: 31
    '    Code Lines: 19
    ' Comment Lines: 3
    '   Blank Lines: 9
    '     File Size: 746 B


    '     Class PipeStream
    ' 
    '         Properties: hashTable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetValue
    ' 
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace MMFProtocol.Pipeline

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class PipeStream : Inherits RawStream

        Public Property hashTable As Dictionary(Of String, PipeBuffer)

        Sub New(raw As Byte())

        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Throw New NotImplementedException()
        End Sub

        Public Shared Function GetValue(raw As Byte(), name As String) As PipeBuffer
            Dim i As Long = Scan0

            Do While True
                Dim buffer As Byte() = raw
            Loop

            Return Nothing
        End Function
    End Class
End Namespace

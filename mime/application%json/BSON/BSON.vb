#Region "Microsoft.VisualBasic::635520bbec757c6eaf63ff676e28594e, mime\application%json\BSON\BSON.vb"

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

    '     Module BSONFormat
    ' 
    '         Function: GetBuffer, Load
    ' 
    '         Sub: WriteBuffer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace BSON

    Public Module BSONFormat

        ''' <summary>
        ''' 解析BSON
        ''' </summary>
        ''' <param name="buf"></param>
        ''' <returns></returns>
        Public Function Load(buf As Byte()) As JsonObject
            Using decoder As New Decoder(buf)
                Return decoder.decodeDocument()
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Sub WriteBuffer(obj As JsonObject, buffer As Stream)
            Call New Encoder().encodeDocument(buffer, obj)
        End Sub

        Public Function GetBuffer(obj As JsonObject) As MemoryStream
            Dim ms As New MemoryStream
            WriteBuffer(obj, buffer:=ms)
            Return ms
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::0b7b81e42b19b25de9d6efeff406072a, Microsoft.VisualBasic.Core\Extensions\WebServices\Http\GZStream.vb"

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

    '     Module GZStream
    ' 
    '         Function: UnzipBase64, UnzipStream, ZipAsBase64, ZipStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices

Namespace Net.Http

    Public Module GZStream

        ''' <summary>
        ''' Unzip the stream data in the <paramref name="base64"/> string.
        ''' </summary>
        ''' <param name="base64$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function UnzipBase64(base64$) As MemoryStream
            Return Convert.FromBase64String(base64).UnzipStream
        End Function

        <Extension>
        Public Function UnzipStream(stream As IEnumerable(Of Byte)) As MemoryStream
            Dim ms As New MemoryStream
            Dim gz As New GZipStream(New MemoryStream(stream.ToArray), CompressionMode.Decompress)
            Call gz.CopyTo(ms)
            Return ms
        End Function

        <Extension>
        Public Function ZipStream(stream As Stream) As MemoryStream
            Dim ms As New MemoryStream
            Dim gz As New GZipStream(ms, CompressionMode.Compress)
            Call stream.CopyTo(gz)
            Return ms
        End Function

        ''' <summary>
        ''' 将目标流对象之中的数据进行zip压缩，然后转换为base64字符串
        ''' </summary>
        ''' <param name="stream"></param>
        ''' <returns></returns>
        <Extension> Public Function ZipAsBase64(stream As Stream) As String
            Dim bytes As Byte() = stream.ZipStream.ToArray
            Dim s$ = Convert.ToBase64String(bytes)
            Return s
        End Function
    End Module
End Namespace

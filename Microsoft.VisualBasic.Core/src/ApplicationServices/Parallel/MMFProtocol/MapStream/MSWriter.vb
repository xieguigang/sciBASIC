﻿#Region "Microsoft.VisualBasic::39d4ffaa8d59f707728f3242d51360e4, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\MMFProtocol\MapStream\MSWriter.vb"

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
    '    Code Lines: 56 (62.92%)
    ' Comment Lines: 18 (20.22%)
    '    - Xml Docs: 94.44%
    ' 
    '   Blank Lines: 15 (16.85%)
    '     File Size: 3.20 KB


    '     Class MSWriter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: WriteStream
    ' 
    '     Class MMFStream
    ' 
    '         Properties: byteData, udtBadge
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace Parallel.MMFProtocol.MapStream

#Region "Nested Type Definitions"

    ''' <summary>
    ''' Memory stream writer
    ''' </summary>
    ''' <remarks>
    ''' mmfServer的主要功能是创建并维护一个内存映射文件
    ''' </remarks>
    Public Class MSWriter : Inherits MapStream

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uri"></param>
        ''' <remarks>对象实例会首先尝试以服务器的角色建立连接，当不成功的时候会以客户端的形式建立连接</remarks>
        Sub New(uri As String, chunkSize As Long)
            Call MyBase.New(uri, chunkSize)

            Try
                file = MemoryMappedFiles.MemoryMappedFile.CreateNew(uri, chunkSize)
            Catch ex As Exception
                Call App.LogException(New Exception(uri, ex))
#If WINDOWS Then
                file = MemoryMappedFiles.MemoryMappedFile.CreateOrOpen(uri, chunkSize)
#Else
                Throw New NotSupportedException("MemoryMappedFile is not supported on unix system")
#End If
            Finally
            End Try
        End Sub

        Public Sub WriteStream(byteData As Byte())
            If Not byteData.IsNullOrEmpty Then
                Call file.CreateViewStream.Write(
                    byteData, Scan0, byteData.Length)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return URI
        End Function
    End Class

    <Serializable> Public Class MMFStream : Inherits RawStream

        ''' <summary>
        ''' Stamp; Update Badge
        ''' </summary>
        Public Property udtBadge As Long

        ''' <summary>
        ''' 内存映射文件中所存储的将要进行进程间交换的数据
        ''' </summary>
        ''' <remarks></remarks>
        Public Property byteData As Byte()

        Sub New()
        End Sub

        Sub New(rawStream As Byte())
            Dim bytTmp As Byte() = New Byte(INT64 - 1) {}
            Call Array.ConstrainedCopy(rawStream, Scan0, bytTmp, Scan0, bytTmp.Length)
            udtBadge = BitConverter.ToInt64(bytTmp, Scan0)
            Dim bytLen As Integer = rawStream.Length - bytTmp.Length
            bytTmp = New Byte(bytLen - 1) {}
            Call Array.ConstrainedCopy(rawStream, INT64, bytTmp, Scan0, bytLen)
            byteData = bytTmp
        End Sub

        Public Overrides Function ToString() As String
            Return $"{NameOf(udtBadge)}:={udtBadge} // {NameOf(byteData)}:={byteData.Length} bytes...."
        End Function

        Public Overrides Sub Serialize(buffer As Stream)
            Dim chunkBuffer As Byte() = New Byte(INT64 + byteData.Length - 1) {}

            Call Array.ConstrainedCopy(BitConverter.GetBytes(udtBadge), Scan0, chunkBuffer, Scan0, INT64)
            Call Array.ConstrainedCopy(byteData, Scan0, chunkBuffer, INT64, byteData.Length)
            Call buffer.Write(chunkBuffer, Scan0, chunkBuffer.Length)
            Call buffer.Flush()
        End Sub
    End Class
#End Region
End Namespace

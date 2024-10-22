#Region "Microsoft.VisualBasic::b9a8ea37a6d8a2df83f45fe4bcddcf33, mime\application%json\BSON\ValueType.vb"

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

'   Total Lines: 16
'    Code Lines: 15 (93.75%)
' Comment Lines: 0 (0.00%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 1 (6.25%)
'     File Size: 340 B


'     Enum ValueType
' 
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Namespace BSON

    Public Enum ValueType As Byte
        [Double] = &H1
        [String] = &H2
        Document = &H3
        Array = &H4
        Binary = &H5
        ''' <summary>
        ''' undefined, obsolete in bson
        ''' </summary>
        Undefined = &H6
        ''' <summary>
        ''' 
        ''' </summary>
        ObjectId = &H7
        [Boolean] = &H8
        UTCDateTime = &H9
        ''' <summary>
        ''' NULL
        ''' </summary>
        None = &HA
        Int32 = &H10
        Int64 = &H12
        [Object] = Document
    End Enum

    Public Class ObjectId

        ' 时间戳部分（4 字节）
        Public Property timestamp As Integer
        ' 机器标识符部分（5 字节）
        Public Property machineIdentifier As Byte()
        ' 进程标识符部分（3 字节）
        Public Property processIdentifier As Byte()
        ' 计数器部分（3 字节）
        Public Property counter As Integer

        Public Sub New(timestamp As Integer, machineIdentifier As Byte(), processIdentifier As Byte(), counter As Integer)
            Me.timestamp = timestamp
            Me.machineIdentifier = machineIdentifier
            Me.processIdentifier = processIdentifier
            Me.counter = counter
        End Sub

        Public Shared Function ReadIdValue(s As BinaryReader) As JsonValue
            ' 读取 4 字节的时间戳
            Dim timestamp As Integer = s.ReadInt32()

            ' 读取 5 字节的机器标识符
            Dim machineIdentifier(4) As Byte
            s.Read(machineIdentifier, 0, 5)

            ' 读取 3 字节的进程标识符
            Dim processIdentifier(2) As Byte
            s.Read(processIdentifier, 0, 3)

            ' 读取 3 字节的计数器，并将其转换为整数
            Dim counterBytes(2) As Byte
            s.Read(counterBytes, 0, 3)
            Dim counter As Integer = BitConverter.ToInt32(New Byte() {0, counterBytes(0), counterBytes(1), counterBytes(2)}, 0)

            ' 创建并返回 ObjectId 对象
            Return New JsonValue(New ObjectId(timestamp, machineIdentifier, processIdentifier, counter))
        End Function

    End Class
End Namespace

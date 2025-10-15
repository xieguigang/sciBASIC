#Region "Microsoft.VisualBasic::f6636994d32c64b60b6a94b17f20847b, vs_solutions\dev\VisualStudio\Resource\MimeData.vb"

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

    '   Total Lines: 35
    '    Code Lines: 18 (51.43%)
    ' Comment Lines: 9 (25.71%)
    '    - Xml Docs: 55.56%
    ' 
    '   Blank Lines: 8 (22.86%)
    '     File Size: 1.17 KB


    '     Module MimeData
    ' 
    '         Function: SerializeBitmapToBase64
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NET48 Then

Imports System.Drawing
Imports System.IO
Imports System.Runtime.Serialization.Formatters.Binary

Namespace Resource

    Public Module MimeData

        ''' <summary>
        ''' 将Bitmap对象序列化为Base64字符串
        ''' </summary>
        ''' <param name="bmp">要序列化的Bitmap对象</param>
        ''' <returns>Base64编码的字符串</returns>
        Public Function SerializeBitmapToBase64(bmp As Bitmap) As String
            Dim base64String As String = String.Empty
            ' 使用BinaryFormatter进行二进制序列化
            Dim binaryFormatter As New BinaryFormatter()

            ' 使用MemoryStream作为序列化的中间存储
            Using memoryStream As New MemoryStream()
                ' 将Bitmap对象序列化到内存流中
                binaryFormatter.Serialize(memoryStream, bmp)

                ' 将内存流中的二进制数据转换为Base64字符串
                base64String = Convert.ToBase64String(memoryStream.ToArray())
            End Using

            Return base64String
        End Function
    End Module
End Namespace

#End If

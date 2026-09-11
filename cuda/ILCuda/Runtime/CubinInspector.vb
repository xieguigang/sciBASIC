#Region "Microsoft.VisualBasic::946dacbdd2f6d5f9fb3044b178693007, cuda\ILCuda\Runtime\CubinInspector.vb"

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
    '    Code Lines: 49 (55.06%)
    ' Comment Lines: 24 (26.97%)
    '    - Xml Docs: 25.00%
    ' 
    '   Blank Lines: 16 (17.98%)
    '     File Size: 4.00 KB


    '     Module CubinInspector
    ' 
    '         Function: IsBinaryImage, IsCompatibleWithDriver, TryGetToolkitVersion
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 预编译镜像（cubin / fatbin）的兼容性预检
'
' 为什么需要它：
'   cuModuleLoadData 只接受一个指针，没有长度参数；当镜像由比驱动更新的
'   CUDA 工具包编译（例如工具包 13.3、驱动只支持 12.8）时，驱动不是返回
'   错误码，而是直接在内部崩溃（0xC0000005），整个进程会被带走。
'
'   好消息是 ELF 镜像的 .note.nv.tkinfo 段里带有编译工具包版本字符串
'   （"Cuda compilation tools, release 13.3, V13.3.73"），
'   因此我们可以在调用驱动之前先判断一次，不匹配就直接跳过。
' ------------------------------------------------------------------------

Imports System.Text
Imports System.Text.RegularExpressions

Namespace Runtime

    Public Module CubinInspector

        ''' <summary>从 ELF/fatbin 镜像中提取编译它的 CUDA 工具包版本</summary>
        Public Function TryGetToolkitVersion(image As Byte(), ByRef major As Integer, ByRef minor As Integer) As Boolean
            major = 0
            minor = 0

            If image Is Nothing OrElse image.Length < 16 Then Return False
            If Not IsBinaryImage(image) Then Return False

            Dim text = Encoding.ASCII.GetString(image)
            Dim match = Regex.Match(text, "release\s+(\d+)\.(\d+)")

            If Not match.Success Then
                match = Regex.Match(text, "cuda_(\d+)\.(\d+)")
            End If
            If Not match.Success Then Return False

            If Not Integer.TryParse(match.Groups(1).Value, major) Then Return False
            If Not Integer.TryParse(match.Groups(2).Value, minor) Then Return False

            Return major > 0
        End Function

        ''' <summary>判断是否为 ELF/fatbin 这类二进制镜像（PTX 文本返回 False）</summary>
        Public Function IsBinaryImage(image As Byte()) As Boolean
            If image Is Nothing OrElse image.Length < 8 Then Return False
            ' ELF 魔数 0x7F 'E' 'L' 'F'
            If image(0) = &H7F AndAlso image(1) = &H45 AndAlso image(2) = &H4C AndAlso image(3) = &H46 Then
                Return True
            End If
            ' fatbin 魔数 0xBA55ED50（小端 50 ED 55 BA）
            If image(0) = &H50 AndAlso image(1) = &HED AndAlso image(2) = &H55 AndAlso image(3) = &HBA Then
                Return True
            End If
            Return False
        End Function

        ''' <summary>
        ''' 判断镜像是否可能被当前驱动加载。
        ''' 无法确定版本（例如 PTX 文本）时返回 True，交由驱动自己判断。
        ''' </summary>
        Public Function IsCompatibleWithDriver(image As Byte(), ByRef reason As String) As Boolean
            reason = Nothing

            Dim major As Integer, minor As Integer
            If Not TryGetToolkitVersion(image, major, minor) Then Return True

            Dim driverKey As Integer
            Try
                driverKey = CudaDevice.DriverVersionKey()
            Catch
                Return True
            End Try

            ' 实测结论：
            '   - 跨 CUDA 大版本的镜像（工具包 13.x + 只支持 12.8 的驱动）会让驱动在
            '     cuModuleLoadData 内部直接崩溃，必须拦截；
            '   - 同一大版本内的次版本差异（工具包 13.3 + 支持 13.2 的驱动）可以正常加载。
            Dim driverMajor = driverKey \ 10

            If major > driverMajor Then
                reason = $"镜像由 CUDA {major}.{minor} 编译，而当前驱动只支持 CUDA {driverKey \ 10}.{driverKey Mod 10}（跨大版本），" &
                         $"已跳过加载（强行加载会导致驱动崩溃）"
                Return False
            End If

            Return True
        End Function
    End Module
End Namespace


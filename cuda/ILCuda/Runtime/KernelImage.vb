#Region "Microsoft.VisualBasic::45af64b0c1890a4773e753ffef334247, cuda\ILCuda\Runtime\KernelImage.vb"

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

    '   Total Lines: 21
    '    Code Lines: 14 (66.67%)
    ' Comment Lines: 5 (23.81%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (9.52%)
    '     File Size: 870 B


    '     Class KernelImage
    ' 
    '         Properties: Arch, Detail, Image, Source
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime

    ''' <summary>一份可以直接喂给 cuModuleLoadData 的内核镜像</summary>
    Public Class KernelImage
        ''' <summary>PTX 文本或 cubin 二进制</summary>
        Public Property Image As Byte()
        ''' <summary>镜像来源描述，例如 "NVRTC 11.2"</summary>
        Public Property Source As String
        ''' <summary>编译使用的 -arch，例如 compute_86</summary>
        Public Property Arch As String
        ''' <summary>补充信息（动态库路径或文件路径）</summary>
        Public Property Detail As String

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(Arch) Then
                Return $"{Source} [{Detail}]"
            End If
            Return $"{Source} arch={Arch} [{Detail}]"
        End Function
    End Class
End Namespace

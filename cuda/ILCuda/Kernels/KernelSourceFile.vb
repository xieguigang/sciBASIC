#Region "Microsoft.VisualBasic::06127b0645bc99aac1a16333f5574b33, cuda\ILCuda\Kernels\KernelSourceFile.vb"

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

    '   Total Lines: 63
    '    Code Lines: 33 (52.38%)
    ' Comment Lines: 18 (28.57%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 12 (19.05%)
    '     File Size: 2.55 KB


    '     Class KernelSourceFile
    ' 
    '         Properties: FileName, Key, Name, Origin, Text
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Kernels

    ''' <summary>
    ''' 一个内核源码文件（通常来自程序集的内嵌资源，也可以是运行时注册的内联源码）
    ''' </summary>
    Public Class KernelSourceFile

        ''' <summary>资源全名，例如 <c>Enigma.ILCuda.Kernels.basic.cu</c></summary>
        Public Property Name As String

        ''' <summary>源码文本</summary>
        Public Property Text As String

        ''' <summary>
        ''' 来源标识：框架自带为 <see cref="KernelSources.BuiltinOrigin"/>，
        ''' 外部注册时默认取程序集名。
        ''' </summary>
        Public Property Origin As String

        ''' <summary>去重键：来源 + 资源名</summary>
        Public ReadOnly Property Key As String
            Get
                Return $"{If(Origin, String.Empty)}::{If(Name, String.Empty)}"
            End Get
        End Property

        ''' <summary>
        ''' 由资源名推导出的文件名（basic.cu）。
        ''' 资源名形如 <c>xxx.Kernels.basic.cu</c>，这里取 "Kernels." 之后的部分；
        ''' 推导失败时退回资源名本身。
        ''' </summary>
        Public ReadOnly Property FileName As String
            Get
                Dim full = If(Name, String.Empty)

                ' 资源名形如 <RootNamespace>.<子目录>.xxx.cu，这里尽量还原成 xxx.cu：
                '   1) 优先取 "Kernels." 之后的部分；
                '   2) 否则取最后两段（MSBuild 默认会把子目录丢掉，只剩 根命名空间.xxx.cu）。
                Dim idx = full.LastIndexOf("Kernels.", StringComparison.Ordinal)

                If idx >= 0 Then
                    full = full.Substring(idx + 8)
                Else
                    Dim parts = full.Split("."c)

                    If parts.Length >= 3 AndAlso
                       parts(parts.Length - 1).Equals("cu", StringComparison.OrdinalIgnoreCase) Then
                        full = parts(parts.Length - 2) & "." & parts(parts.Length - 1)
                    End If
                End If

                If String.IsNullOrWhiteSpace(full) Then full = If(Name, "kernel.cu")
                If Not full.EndsWith(".cu", StringComparison.OrdinalIgnoreCase) Then full &= ".cu"

                Return full
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Origin}/{FileName}"
        End Function
    End Class
End Namespace

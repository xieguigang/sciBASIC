#Region "Microsoft.VisualBasic::a39325e2fff870a383054c66ec7c14f2, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\SIMD.vb"

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

    '   Total Lines: 65
    '    Code Lines: 23 (35.38%)
    ' Comment Lines: 34 (52.31%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (12.31%)
    '     File Size: 2.30 KB


    '     Enum SIMDConfiguration
    ' 
    '         auto, disable, enable, legacy
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class SIMDEnvironment
    ' 
    '         Properties: config
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics

#If Not NET48 Then
Imports System.Runtime.Intrinsics
Imports System.Runtime.Intrinsics.X86
#End If

Namespace Math.SIMD

    Public Enum SIMDConfiguration
        ''' <summary>
        ''' no SIMD
        ''' </summary>
        disable
        ''' <summary>
        ''' use the new .netcore SIMD supports from ``System.Runtime.Intrinsics.X86``
        ''' </summary>
        enable
        ''' <summary>
        ''' use the legacy supports of the SIMD from ``System.Numerics``
        ''' </summary>
        legacy

        ''' <summary>
        ''' disable or SIMD **legacy** mode based on the vector size
        ''' </summary>
        auto
    End Enum

    ''' <summary>
    ''' SIMD(Single Instruction Multiple Data)即单指令流多数据流，
    ''' 是一种采用一个控制器来控制多个处理器，同时对一组数据（又称“数据
    ''' 向量”）中的每一个分别执行相同的操作从而实现空间上的并行性的技术。
    ''' 简单来说就是一个指令能够同时处理多个数据。
    ''' </summary>
    ''' <remarks>
    ''' 在这个模块中的代码都不会进行安全检查，默认都是符合计算条件的
    ''' </remarks>
    Public NotInheritable Class SIMDEnvironment

        ''' <summary>
        ''' This option only works for .NET core runtime
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property config As SIMDConfiguration = SIMDConfiguration.auto

        ''' <summary>
        ''' Vector(Of <see cref="Double"/>).Count
        ''' </summary>
        Public Shared ReadOnly countDouble As Integer = Vector(Of Double).Count
        ''' <summary>
        ''' Vector(Of <see cref="Single"/>).Count
        ''' </summary>
        Public Shared ReadOnly countFloat As Integer = Vector(Of Single).Count
        ''' <summary>
        ''' Vector(Of <see cref="Integer"/>).Count
        ''' </summary>
        Public Shared ReadOnly countInteger As Integer = Vector(Of Integer).Count
        Public Shared ReadOnly countLong As Integer = Vector(Of Long).Count
        Public Shared ReadOnly countShort As Integer = Vector(Of Short).Count

        Private Sub New()
        End Sub
    End Class
End Namespace

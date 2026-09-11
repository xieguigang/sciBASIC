#Region "Microsoft.VisualBasic::cd2a7f46631863181574690715021832, Microsoft.VisualBasic.Core\src\Extensions\Math\SIMD\Arithmetic\Add.vb"

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

    '   Total Lines: 71
    '    Code Lines: 28 (39.44%)
    ' Comment Lines: 34 (47.89%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (12.68%)
    '     File Size: 2.70 KB


    '     Class Add
    ' 
    '         Function: f32_op_add_f32, f32_op_add_f32_scalar, f64_op_add_f64, f64_op_add_f64_scalar, int32_op_add_int32
    '                   int32_op_add_int32_scalar, int64_op_add_int64, int64_op_add_int64_scalar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Math.SIMD

    ''' <summary>
    ''' 逐元素加法。
    ''' </summary>
    ''' <remarks>
    ''' 这个类型原有的公开函数（``f64_*``）全部被保留下来并改写为
    ''' <see cref="SimdEngine"/> 的薄封装，因此既有的调用点无需任何改动即可
    ''' 直接获得新的向量化实现。
    ''' </remarks>
    Public Class Add

        ''' <summary>
        ''' 向量加标量：<c>v1(i) + v2</c>
        ''' </summary>
        ''' <param name="v1"></param>
        ''' <param name="v2"></param>
        Public Shared Function f64_op_add_f64_scalar(v1 As Double(), v2 As Double) As Double()
            Return SimdEngine.AddScalar(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加向量：<c>v1(i) + v2(i)</c>
        ''' </summary>
        Public Shared Function f64_op_add_f64(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.Add(Of Double)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加标量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_op_add_f32_scalar(v1 As Single(), v2 As Single) As Single()
            Return SimdEngine.AddScalar(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加向量（<see cref="Single"/>）
        ''' </summary>
        Public Shared Function f32_op_add_f32(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Add(Of Single)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加标量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_add_int32_scalar(v1 As Integer(), v2 As Integer) As Integer()
            Return SimdEngine.AddScalar(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加向量（<see cref="Integer"/>）
        ''' </summary>
        Public Shared Function int32_op_add_int32(v1 As Integer(), v2 As Integer()) As Integer()
            Return SimdEngine.Add(Of Integer)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加标量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_add_int64_scalar(v1 As Long(), v2 As Long) As Long()
            Return SimdEngine.AddScalar(Of Long)(v1, v2)
        End Function

        ''' <summary>
        ''' 向量加向量（<see cref="Long"/>）
        ''' </summary>
        Public Shared Function int64_op_add_int64(v1 As Long(), v2 As Long()) As Long()
            Return SimdEngine.Add(Of Long)(v1, v2)
        End Function
    End Class
End Namespace

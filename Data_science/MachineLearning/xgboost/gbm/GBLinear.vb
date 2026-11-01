#Region "Microsoft.VisualBasic::c758c7ac8aa41616a98265de5a9d9112, Data_science\MachineLearning\xgboost\gbm\GBLinear.vb"

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

    '   Total Lines: 96
    '    Code Lines: 65 (67.71%)
    ' Comment Lines: 11 (11.46%)
    '    - Xml Docs: 27.27%
    ' 
    '   Blank Lines: 20 (20.83%)
    '     File Size: 3.47 KB


    '     Class GBLinear
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: bias, pred, predict, predictLeaf, predictSingle
    '                   weight
    ' 
    '         Sub: loadModel
    '         Class ModelParam
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train.kernels
Imports sysmath = System.Math

Namespace gbm

    ''' <summary>
    ''' Linear booster implementation
    ''' </summary>
    <Serializable>
    Public Class GBLinear
        Inherits GBBase

        Private mparam As ModelParam
        Private weights As Single()

        Friend Sub New()
            ' do nothing
        End Sub

        Public Overrides Sub loadModel(reader As ModelReader, ignored_with_pbuffer As Boolean)
            mparam = New ModelParam(reader)
            reader.readInt() ' read padding
            weights = reader.readFloatArray((mparam.num_feature + 1) * mparam.num_output_group)
        End Sub

        Public Overrides Function predict(feat As FVec, ntree_limit As Integer) As Double()
            Dim preds = New Double(mparam.num_output_group - 1) {}

            ' 旧实现在自增局部变量时使用 Interlocked.Increment：每特征一次完整内存屏障，
            ' 直接把线性 booster 的评分成本抬高一个量级。
            For gid As Integer = 0 To preds.Length - 1
                preds(gid) = pred(feat, gid)
            Next

            Return preds
        End Function

        Public Overrides Function predictSingle(feat As FVec, ntree_limit As Integer) As Double
            If mparam.num_output_group <> 1 Then
                Throw New InvalidOperationException("Can't invoke predictSingle() because this model outputs multiple values: " & mparam.num_output_group)
            End If

            Return pred(feat, 0)
        End Function

        Friend Overridable Function pred(feat As FVec, gid As Integer) As Double
            ' 稠密 Single 向量 + 单输出组：权重按 fid 连续存放，可整段 SIMD 点积
            Dim dense As Single() = denseValues(feat)

            If dense IsNot Nothing AndAlso mparam.num_output_group = 1 AndAlso gid = 0 Then
                Dim n As Integer = sysmath.Min(mparam.num_feature, dense.Length)

                Return bias(0) + SimdOps.DotSingle(dense, weights, n)
            End If

            Dim psum As Double = bias(gid)
            Dim featValue As Double

            For fid As Integer = 0 To mparam.num_feature - 1
                featValue = feat.fvalue(fid)

                If Not Double.IsNaN(featValue) Then
                    psum += featValue * weight(fid, gid)
                End If
            Next

            Return psum
        End Function

        Public Overrides Function predictLeaf(feat As FVec, ntree_limit As Integer) As Integer()
            Throw New NotSupportedException("gblinear does not support predict leaf index")
        End Function

        ''' <summary>
        ''' 取稠密向量实现背后的原始数组（稀疏映射实现返回 <c>Nothing</c>）
        ''' </summary>
        Private Shared Function denseValues(feat As FVec) As Single()
            Dim impl = TryCast(feat, FVecArray.FVecFloatArrayImpl)

            If impl Is Nothing Then
                Return Nothing
            End If

            Return impl.values
        End Function

        Friend Overridable Function weight(fid As Integer, gid As Integer) As Single
            Return weights(fid * mparam.num_output_group + gid)
        End Function

        Friend Overridable Function bias(gid As Integer) As Single
            Return weights(mparam.num_feature * mparam.num_output_group + gid)
        End Function

        <Serializable>
        Friend Class ModelParam
            ' ! \brief number of features 
            Friend ReadOnly num_feature As Integer
            ' !
            '  \brief how many output group a single instance can produce
            '   this affects the behavior of number of output we have:
            '     suppose we have n instance and k group, output will be k*n
            ' 
            Friend ReadOnly num_output_group As Integer
            ' ! \brief reserved parameters 
            Friend ReadOnly reserved As Integer()

            Friend Sub New(reader As ModelReader)
                num_feature = reader.readInt()
                num_output_group = reader.readInt()
                reserved = reader.readIntArray(32)
                reader.readInt() ' read padding
            End Sub
        End Class
    End Class
End Namespace

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
            Dim gid = 0

            While gid < mparam.num_output_group
                preds(gid) = pred(feat, gid)
                Threading.Interlocked.Increment(gid)
            End While

            Return preds
        End Function

        Public Overrides Function predictSingle(feat As FVec, ntree_limit As Integer) As Double
            If mparam.num_output_group <> 1 Then
                Throw New InvalidOperationException("Can't invoke predictSingle() because this model outputs multiple values: " & mparam.num_output_group)
            End If

            Return pred(feat, 0)
        End Function

        Friend Overridable Function pred(feat As FVec, gid As Integer) As Double
            Dim psum As Double = bias(gid)
            Dim featValue As Double
            Dim fid = 0

            While fid < mparam.num_feature
                featValue = feat.fvalue(fid)

                If Not Double.IsNaN(featValue) Then
                    psum += featValue * weight(fid, gid)
                End If

                Threading.Interlocked.Increment(fid)
            End While

            Return psum
        End Function

        Public Overrides Function predictLeaf(feat As FVec, ntree_limit As Integer) As Integer()
            Throw New NotSupportedException("gblinear does not support predict leaf index")
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

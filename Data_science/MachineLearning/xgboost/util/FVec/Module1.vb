#Region "Microsoft.VisualBasic::e849e492af2ec50b9988965385039204, Data_science\MachineLearning\xgboost\util\FVec\Module1.vb"

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

    '   Total Lines: 111
    '    Code Lines: 82 (73.87%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 29 (26.13%)
    '     File Size: 3.14 KB


    '     Class FVecFloatArrayImpl
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: fvalue
    ' 
    '     Class FVecFloatArrayImplement
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: fvalue
    ' 
    '     Class FVecDoubleArrayImpl
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: fvalue
    ' 
    '     Class FVecDoubleArrayImplement
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: fvalue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace util.FVecArray

    <Serializable>
    Friend Class FVecFloatArrayImpl
        Implements FVec

        Friend ReadOnly values As Single()
        Friend ReadOnly treatsZeroAsNA As Boolean

        Friend Sub New(values As Single(), treatsZeroAsNA As Boolean)
            Me.values = values
            Me.treatsZeroAsNA = treatsZeroAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result As Double = values(index)

            If treatsZeroAsNA AndAlso result = 0 Then
                Return Double.NaN
            End If

            Return result
        End Function
    End Class

    <Serializable>
    Friend Class FVecFloatArrayImplement
        Implements FVec

        Friend ReadOnly values As Single()
        Friend ReadOnly treatsValueAsNA As Single

        Friend Sub New(values As Single(), treatsValueAsNA As Single)
            Me.values = values
            Me.treatsValueAsNA = treatsValueAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result As Double = values(index)

            If treatsValueAsNA = result Then
                Return Double.NaN
            End If

            Return result
        End Function
    End Class

    <Serializable>
    Friend Class FVecDoubleArrayImpl
        Implements FVec

        Friend ReadOnly values As Double()
        Friend ReadOnly treatsZeroAsNA As Boolean

        Friend Sub New(values As Double(), treatsZeroAsNA As Boolean)
            Me.values = values
            Me.treatsZeroAsNA = treatsZeroAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result = values(index)

            If treatsZeroAsNA AndAlso result = 0 Then
                Return Double.NaN
            End If

            Return values(index)
        End Function
    End Class

    <Serializable>
    Friend Class FVecDoubleArrayImplement
        Implements FVec

        Friend ReadOnly values As Double()
        Friend ReadOnly treatsValueAsNA As Double

        Friend Sub New(values As Double(), treatsValueAsNA As Double)
            Me.values = values
            Me.treatsValueAsNA = treatsValueAsNA
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            If values.Length <= index Then
                Return Double.NaN
            End If

            Dim result = values(index)

            If treatsValueAsNA = result Then
                Return Double.NaN
            End If

            Return values(index)
        End Function
    End Class

End Namespace

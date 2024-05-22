#Region "Microsoft.VisualBasic::6746a073d463fb1bb0933f4126b4d5ed, Data_science\MachineLearning\xgboost\util\DMatrix.vb"

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

    '   Total Lines: 45
    '    Code Lines: 37 (82.22%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (17.78%)
    '     File Size: 1.44 KB


    '     Class DMatrix
    ' 
    '         Properties: label, matrix, size
    ' 
    '         Function: Builder, enumerateData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace util

    Public Class DMatrix

        Public Property label As Integer()
        Public Property matrix As FVec()

        Public ReadOnly Property size As Integer
            Get
                Return matrix.Length
            End Get
        End Property

        Public Iterator Function enumerateData() As IEnumerable(Of IntegerTagged(Of FVec))
            For i As Integer = 0 To size - 1
                Yield New IntegerTagged(Of FVec) With {
                    .Tag = label(i),
                    .Value = matrix(i)
                }
            Next
        End Function

        Public Shared Function Builder() As (add As Action(Of Integer, FVec), getMatrix As Func(Of DMatrix))
            Dim labels As New List(Of Integer)
            Dim matrix As New List(Of FVec)
            Dim add As Action(Of Integer, FVec) =
                Sub(label, vec)
                    Call labels.Add(label)
                    Call matrix.Add(vec)
                End Sub
            Dim getMatrix As Func(Of DMatrix) =
                Function()
                    Return New DMatrix With {
                        .label = labels.ToArray,
                        .matrix = matrix.ToArray
                    }
                End Function

            Return (add, getMatrix)
        End Function

    End Class
End Namespace

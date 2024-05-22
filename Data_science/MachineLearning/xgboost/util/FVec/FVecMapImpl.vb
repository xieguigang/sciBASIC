#Region "Microsoft.VisualBasic::695a07f681a4310aa76df5f2851285f6, Data_science\MachineLearning\xgboost\util\FVec\FVecMapImpl.vb"

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

    '   Total Lines: 26
    '    Code Lines: 17 (65.38%)
    ' Comment Lines: 4 (15.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (19.23%)
    '     File Size: 776 B


    '     Class FVecMapImpl
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: fvalue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace util

    ''' <summary>
    ''' 稀疏矩阵
    ''' </summary>
    ''' <typeparam name="T1"></typeparam>
    <Serializable>
    Friend Class FVecMapImpl(Of T1 As IComparable) : Implements FVec

        Private ReadOnly values As IDictionary(Of String, T1)

        Public Sub New(values As IDictionary(Of String, T1))
            Me.values = values
        End Sub

        Public Overridable Function fvalue(index As Integer) As Double Implements FVec.fvalue
            Dim number As IComparable = values.GetValueOrNull(index.ToString)

            If number Is Nothing Then
                Return Double.NaN
            Else
                Return CType(number, Double)
            End If
        End Function
    End Class
End Namespace

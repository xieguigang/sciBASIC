#Region "Microsoft.VisualBasic::4da00d4a65e46941a82760e389d5bf69, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\MantelTest\Model.vb"

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

    '   Total Lines: 76
    '    Code Lines: 29 (38.16%)
    ' Comment Lines: 35 (46.05%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 12 (15.79%)
    '     File Size: 2.08 KB


    '     Class Result
    ' 
    '         Properties: coef, numelt, proba
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class Model
    ' 
    '         Properties: [partial], exact, matsize, numrand, raw
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Hypothesis.Mantel

    Public Class Result : Inherits Model

        ''' <summary>
        ''' [result] reference statistic
        ''' </summary>
        ''' <returns></returns>
        Public Property coef As Double

        ''' <summary>
        ''' [result] p-value
        ''' </summary>
        ''' <returns></returns>
        Public Property proba As Double

        ''' <summary>
        ''' number of elements in the half-matrix without diagonal values
        ''' </summary>
        ''' <returns></returns>
        Public Property numelt As Integer

        <DebuggerStepThrough>
        Sub New(copyModel As Model)
            matsize = copyModel.matsize
            [partial] = copyModel.partial
            raw = copyModel.raw
            exact = copyModel.exact
            numrand = copyModel.numrand
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class

    ''' <summary>
    ''' the test model
    ''' </summary>
    Public Class Model

        ''' <summary>
        ''' size of matrices
        ''' </summary>
        ''' <returns></returns>
        Public Property matsize As Integer
        ''' <summary>
        ''' option partial 0|1
        ''' </summary>
        ''' <returns></returns>
        Public Property [partial] As Boolean
        ''' <summary>
        ''' option raw 0|1
        ''' </summary>
        ''' <returns></returns>
        Public Property raw As Boolean
        ''' <summary>
        ''' option exact permutation 0|1
        ''' </summary>
        ''' <returns></returns>
        Public Property exact As Boolean
        ''' <summary>
        ''' [permutations] number of randomizations
        ''' </summary>
        ''' <returns></returns>
        Public Property numrand As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace

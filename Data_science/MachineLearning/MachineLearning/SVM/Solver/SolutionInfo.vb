#Region "Microsoft.VisualBasic::6e4dda3676e40c917b02951fb547566b, Data_science\MachineLearning\MachineLearning\SVM\Solver\SolutionInfo.vb"

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

    '   Total Lines: 48
    '    Code Lines: 13 (27.08%)
    ' Comment Lines: 30 (62.50%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 5 (10.42%)
    '     File Size: 1.84 KB


    '     Class SolutionInfo
    ' 
    '         Properties: obj, r, rho, upper_bound_n, upper_bound_p
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace SVM

    ' java: information about solution except alpha,
    ' because we cannot return multiple values otherwise...
    ''' <summary>
    ''' The solution information of the support vector machine solver, which 
    ''' contains everything of the solution except the alpha coefficients.
    ''' </summary>
    Public Class SolutionInfo

        ''' <summary>
        ''' The value of the objective function which was minimized.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property obj As Double
        ''' <summary>
        ''' The bias term of the decision function.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property rho As Double
        ''' <summary>
        ''' The upper bound value of the positive side lagrange multiplier.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property upper_bound_p As Double
        ''' <summary>
        ''' The upper bound value of the negative side lagrange multiplier.
        ''' </summary>
        ''' <returns>A <see cref="Double"/> value.</returns>
        Public Property upper_bound_n As Double

        ''' <summary>
        ''' for Solver_NU
        ''' </summary>
        ''' <returns>The <i>r</i> value which is only used by the nu solver.</returns>
        Public Property r As Double

        ''' <summary>
        ''' Display this solution information as a json string.
        ''' </summary>
        ''' <returns>A json text which describes this solution information.</returns>
        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

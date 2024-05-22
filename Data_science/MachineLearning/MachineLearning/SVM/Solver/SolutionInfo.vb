#Region "Microsoft.VisualBasic::22b7b7dadb4ea218029ff9342eb54b85, Data_science\MachineLearning\MachineLearning\SVM\Solver\SolutionInfo.vb"

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

    '   Total Lines: 24
    '    Code Lines: 13 (54.17%)
    ' Comment Lines: 6 (25.00%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 5 (20.83%)
    '     File Size: 678 B


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
    Public Class SolutionInfo

        Public Property obj As Double
        Public Property rho As Double
        Public Property upper_bound_p As Double
        Public Property upper_bound_n As Double

        ''' <summary>
        ''' for Solver_NU
        ''' </summary>
        ''' <returns></returns>
        Public Property r As Double

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

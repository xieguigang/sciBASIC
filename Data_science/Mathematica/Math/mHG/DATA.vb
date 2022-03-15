#Region "Microsoft.VisualBasic::a6942910f23bbb1dd6479d47c9df6249, sciBASIC#\Data_science\Mathematica\Math\mHG\DATA.vb"

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
    '    Code Lines: 12
    ' Comment Lines: 28
    '   Blank Lines: 5
    '     File Size: 1.06 KB


    ' Class htest
    ' 
    '     Properties: b, n, parameters, pvalue, statistic
    ' 
    ' Class mHGstatisticInfo
    ' 
    '     Properties: b, mHG, n
    ' 
    ' /********************************************************************************/

#End Region

Public Class htest

    Public Property statistic As Dictionary(Of String, Double)
    Public Property parameters As Dictionary(Of String, Double)
    Public Property pvalue As Double
    Public Property n As Integer
    Public Property b As Double

End Class

''' <summary>
''' mHG definition:
''' 
''' ```
'''   mHG(lambdas) = min over 1 &lt;= n &lt;= N Of HGT (b_n(lambdas); N, B, n)
''' ```
'''   
''' Where ``HGT`` Is the hypergeometric tail:
''' 
''' ```
'''   HGT(b; N, B, n) = Probability(X >= b)
''' ```
'''   
''' And:
''' 
''' ```
'''   b_n = sum over 1 &lt;= i &lt;= n Of lambdas[i]
''' ```
''' </summary>
Public Class mHGstatisticInfo

    ''' <summary>
    ''' the statistic itself
    ''' </summary>
    Public Property mHG As Double
    ''' <summary>
    ''' the index For which it was obtained
    ''' </summary>
    Public Property n As Double
    ''' <summary>
    ''' (Short For b_n) - sum over ``1 &lt;= i &lt;= n`` Of lambdas[i]
    ''' </summary>
    Public Property b As Double

End Class

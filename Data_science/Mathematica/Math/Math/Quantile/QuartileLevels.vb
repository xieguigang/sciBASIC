#Region "Microsoft.VisualBasic::ccb7d51a3448f5c5c9880b17753fd352, Data_science\Mathematica\Math\Math\Quantile\QuartileLevels.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14 (73.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (26.32%)
    '     File Size: 424 B


    '     Enum QuartileLevels
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    '     Structure QuantileThreshold
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Quantile

    Public Enum QuartileLevels As Integer
        Q1 = 1
        Q2 = 2
        Q3 = 3
    End Enum

    Public Structure QuantileThreshold

        Dim quantile As Double
        Dim sample As Double

        Public Overrides Function ToString() As String
            Return $"q:{quantile.ToString("F2")} ~ {sample.ToString("G3")}"
        End Function

    End Structure
End Namespace

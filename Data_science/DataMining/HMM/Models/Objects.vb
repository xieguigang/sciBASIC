#Region "Microsoft.VisualBasic::f2e2a8d37fde7991bd8fe73fadc9adea, Data_science\DataMining\HMM\Models\Objects.vb"

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

    '   Total Lines: 27
    '    Code Lines: 18 (66.67%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (33.33%)
    '     File Size: 725 B


    '     Class StatesObject
    ' 
    '         Properties: prob, state
    ' 
    '         Function: ToString
    ' 
    '     Class Observable
    ' 
    '         Properties: obs, prob
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

Namespace Models

    Public Class StatesObject

        Public Property state As String
        Public Property prob As Double()

        Public Overrides Function ToString() As String
            Return $"{state}: {prob.Select(Function(d) stdNum.Round(d, 3)).ToArray.GetJson}"
        End Function

    End Class

    Public Class Observable

        Public Property obs As String
        Public Property prob As Double()

        Public Overrides Function ToString() As String
            Return $"{obs}: {prob.Select(Function(d) stdNum.Round(d, 3)).ToArray.GetJson}"
        End Function

    End Class
End Namespace

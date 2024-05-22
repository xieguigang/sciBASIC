#Region "Microsoft.VisualBasic::d7e1dcf39ae7973acc07b08b3a3b5d4f, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\T-test\Topt.vb"

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

    '   Total Lines: 21
    '    Code Lines: 11 (52.38%)
    ' Comment Lines: 4 (19.05%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (28.57%)
    '     File Size: 495 B


    '     Class Topt
    ' 
    '         Properties: alpha, alternative, mu
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Hypothesis

    Public Class Topt

        Public Property mu As Double
        Public Property alpha As Double

        ''' <summary>
        ''' the alternative hypothesis.
        ''' </summary>
        ''' <returns></returns>
        Public Property alternative As Hypothesis

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

    End Class
End Namespace

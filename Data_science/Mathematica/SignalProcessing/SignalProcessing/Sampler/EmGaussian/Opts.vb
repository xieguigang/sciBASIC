#Region "Microsoft.VisualBasic::e4d66a79ce1a318d3648cc78d4173503, Data_science\Mathematica\SignalProcessing\SignalProcessing\Sampler\EmGaussian\Opts.vb"

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
    '    Code Lines: 11 (40.74%)
    ' Comment Lines: 12 (44.44%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (14.81%)
    '     File Size: 786 B


    '     Class Opts
    ' 
    '         Properties: eps, maxIterations, maxNumber, tolerance
    ' 
    '         Function: GetDefault
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace EmGaussian

    Public Class Opts

        ''' <summary>
        ''' max number of components in case of auto-detection
        ''' </summary>
        ''' <returns></returns>
        Public Property maxNumber As Integer = 100
        ''' <summary>
        ''' max number of iterations
        ''' </summary>
        ''' <returns></returns>
        Public Property maxIterations As Integer = 100
        ''' <summary>
        ''' min difference of likelihood
        ''' </summary>
        ''' <returns></returns>
        Public Property tolerance As Double = 0.00001

        Public Property eps As Double = 0.0000000001

        Public Shared Function GetDefault() As Opts
            Return New Opts
        End Function
    End Class
End Namespace

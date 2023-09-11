#Region "Microsoft.VisualBasic::95f857ee1b219140f983e143cd3a3b93, sciBASIC#\Data_science\Mathematica\Math\Math\Distributions\Gaussian.vb"

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

'   Total Lines: 29
'    Code Lines: 10
' Comment Lines: 14
'   Blank Lines: 5
'     File Size: 895 B


'     Module Gaussian
' 
'         Function: Gaussian
' 
' 
' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace Distributions

    ''' <summary>
    ''' ##### Gaussian function
    ''' 
    ''' https://en.wikipedia.org/wiki/Gaussian_function
    ''' </summary>
    Public Module Gaussian

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="a#">is the height of the curve's peak</param>
        ''' <param name="b#">is the position of the center of the peak</param>
        ''' <param name="sigma#">(the standard deviation, sometimes called the Gaussian RMS width) 
        ''' controls the width of the "bell"</param>
        ''' <returns></returns>
        Public Function Gaussian(x#, a#, b#, sigma#) As Double
            Dim p# = ((x - b) ^ 2) / (2 * sigma ^ 2)
            Dim fx# = a * std.E ^ (-p)
            Return fx
        End Function

        Public Function StandadizedGaussianFunction(diff As Double, devi As Double) As Double
            ' exp(-(diff/devi)^2 / 2)
            Dim result = std.Exp(-0.5 * std.Pow(diff / devi, 2))
            Return result
        End Function
    End Module
End Namespace

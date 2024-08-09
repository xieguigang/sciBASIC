#Region "Microsoft.VisualBasic::bbad7ee164ecfc13ba78d7a6d0651a8e, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\LogLikelihood.vb"

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

'   Total Lines: 42
'    Code Lines: 24 (57.14%)
' Comment Lines: 12 (28.57%)
'    - Xml Docs: 91.67%
' 
'   Blank Lines: 6 (14.29%)
'     File Size: 1.79 KB


' Module LogLikelihood
' 
'     Function: Likelihood
' 
' /********************************************************************************/

#End Region

Imports std = System.Math

''' <summary>
''' calculates log likelihood between a source and a reference corpus. for more
''' details on LLH please visit http : //ucrel.lancs.ac.uk/llwizard.html
''' </summary>
Public Module LogLikelihood

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="freq1">frequency of the word in your corpus</param>
    ''' <param name="freq2">frequency of the word in the reference corpus</param>
    ''' <param name="corpus1Size">size of your corpus in words</param>
    ''' <param name="corpus2Size">size of the reference corpus in words</param>
    ''' <returns></returns>
    Public Function Likelihood(freq1#, freq2#, corpus1Size#, corpus2Size#) As Double
        Dim e1p1 As Double = (corpus1Size * (freq1 + freq2))
        Dim e1p2 As Double = (corpus1Size + corpus2Size)
        Dim expectedValue1 As Double = e1p1 / e1p2
        Dim e2p1 As Double = (corpus2Size * (freq1 + freq2))
        Dim e2p2 As Double = (corpus1Size + corpus2Size)
        Dim expectedValue2 = e2p1 / e2p2

        If expectedValue1 = 0 OrElse expectedValue2 = 0 OrElse
            Double.IsNaN(expectedValue1) OrElse
            Double.IsNaN(expectedValue1) OrElse
            Double.IsNaN(std.Log(freq1 / expectedValue1)) OrElse
            Double.IsNaN(std.Log(freq2 / expectedValue2)) OrElse
            Double.IsInfinity(std.Log(freq1 / expectedValue1)) OrElse
            Double.IsInfinity(std.Log(freq2 / expectedValue2)) Then

            Return 0.0
        Else
            Dim llhP1 As Double = (freq1 * std.Log(freq1 / expectedValue1))
            Dim llhP2 As Double = (freq2 * std.Log(freq2 / expectedValue2))

            Return 2 * (llhP1 + llhP2)
        End If
    End Function

End Module

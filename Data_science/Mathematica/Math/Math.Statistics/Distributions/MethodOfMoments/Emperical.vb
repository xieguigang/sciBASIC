#Region "Microsoft.VisualBasic::b351871f9bb3b28d49e5cea2bb3e9fc7, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Emperical.vb"

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

    '   Total Lines: 43
    '    Code Lines: 25 (58.14%)
    ' Comment Lines: 12 (27.91%)
    '    - Xml Docs: 25.00%
    ' 
    '   Blank Lines: 6 (13.95%)
    '     File Size: 1.73 KB


    '     Class Emperical
    ' 
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions.MethodOfMoments


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Emperical
        Inherits Distributions.ContinuousDistribution

        Private _CumulativeProbabilities As Double()
        Private _ExceedanceValues As Double()
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Dim index As Integer = Array.BinarySearch(_CumulativeProbabilities, probability)
            'interpolate or step?
            'check for array out of bounds...
            Return _ExceedanceValues(index)
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Dim index As Integer = Array.BinarySearch(_ExceedanceValues, value)
            'interpolate or step?
            Return _CumulativeProbabilities(index)
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _CumulativeProbabilities.Length <> _ExceedanceValues.Length Then
                Yield New Exception("Cumulative Probability values and Emperical Exceedance values are different lengths.")
            End If
        End Function

    End Class

End Namespace

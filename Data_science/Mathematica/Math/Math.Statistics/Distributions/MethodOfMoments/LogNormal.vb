#Region "Microsoft.VisualBasic::46b1ea6012d4bb05af1f6e53888b8584, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\LogNormal.vb"

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

    '   Total Lines: 74
    '    Code Lines: 56 (75.68%)
    ' Comment Lines: 13 (17.57%)
    '    - Xml Docs: 46.15%
    ' 
    '   Blank Lines: 5 (6.76%)
    '     File Size: 3.19 KB


    '     Class LogNormal
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Bullentin17BConfidenceLimit, GetCDF, GetInvCDF, GetPDF, Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

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
    Public Class LogNormal
        Inherits Distributions.ContinuousDistribution

        Private _Mean As Double
        Private _StDev As Double
        Public Sub New()
            'for reflection
            _Mean = 0
            _StDev = 1
        End Sub
        Public Sub New(mean As Double, stdev As Double, samplesize As Integer)
            _Mean = mean
            _StDev = stdev
            PeriodOfRecord = (samplesize)
        End Sub
        ''' <summary>
        ''' This takes an input array of sample data, calculates the log base 10 of the data, then calculates the mean and standard deviation of the log data. </summary>
        ''' <param name="data"> the sampled data (in linear space) </param>
        Public Sub New(data As Double())
            For i As Integer = 0 To data.Length - 1
                data(i) = std.Log10(data(i))
            Next i
            Dim BPM As New MomentFunctions.BasicProductMoments(data)
            _Mean = BPM.Mean()
            _StDev = BPM.StDev()
            PeriodOfRecord = (BPM.SampleSize())
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            Dim z As New Normal(_Mean, _StDev)
            Return std.Pow(10, z.GetInvCDF(probability))
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Dim n As New Distributions.MethodOfMoments.Normal(_Mean, _StDev)
            Return n.GetCDF(std.Log10(value))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Dim n As New Distributions.MethodOfMoments.Normal(_Mean, _StDev)
            Return n.GetPDF(std.Log10(value))
        End Function
        Public Overridable Function Bullentin17BConfidenceLimit(probability As Double, alphaValue As Double) As Double
            Dim sn As New Normal(0, 1)
            Dim k As Double = sn.GetInvCDF(probability)
            Dim z As Double = sn.GetInvCDF(alphaValue)
            Dim zSquared As Double = std.Pow(z, 2)
            Dim kSquared As Double = std.Pow(k, 2)
            Dim Avalue As Double = (1 - (zSquared) / 2 \ (PeriodOfRecord() - 1))
            Dim Bvalue As Double = (kSquared) - ((zSquared) / PeriodOfRecord())
            Dim RootValue As Double = std.Sqrt(kSquared - (Avalue * Bvalue))
            If alphaValue > 0.5 Then
                Return std.Pow(10, _Mean + _StDev * (k + RootValue) / Avalue)
            Else
                Return std.Pow(10, _Mean + _StDev * (k - RootValue) / Avalue)
            End If
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _StDev <= 0 Then Yield New Exception("Standard of Deviation must be greater than 0")
        End Function
    End Class

End Namespace

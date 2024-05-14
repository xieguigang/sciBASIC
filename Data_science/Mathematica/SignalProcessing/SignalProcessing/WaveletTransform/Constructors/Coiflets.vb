#Region "Microsoft.VisualBasic::a72c5d3a980fb316f41d68b7e2337b73, Data_science\Mathematica\SignalProcessing\SignalProcessing\WaveletTransform\Constructors\Coiflets.vb"

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

    '   Total Lines: 37
    '    Code Lines: 34
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 2.75 KB


    '     Class Coiflets
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateAllCoiflets
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace WaveletTransform

    Public Class Coiflets : Inherits WaveletConstructor

        Public Sub New()
            MyBase.New(wavelets:=CreateAllCoiflets)
        End Sub

        Public Shared Iterator Function CreateAllCoiflets() As IEnumerable(Of Wavelet)
            Yield New Wavelet("Coiflet_1", {
            -0.0156557281354645, -0.0727326195128539, 0.384864846864203, 0.852572020212255, 0.337897662457809, -0.0727326195128539
        })
            Yield New Wavelet("Coiflet_2", {
                          -0.000720549445364512, -0.00182320887070299, 0.0056114348193945, 0.0236801719463341, -0.0594344186464569, -0.0764885990783064,
                          0.417005184421693, 0.812723635445542, 0.386110066821162, -0.067372554721963, -0.0414649367817592, 0.0163873364635221
                          })
            Yield New Wavelet("Coiflet_3", {
            -0.0000345997728362126, -0.0000709833031381413, 0.000466216960112886, 0.0011175187708906, -0.00257451768875022, -0.00900797613666158,
            0.0158805448636159, 0.0345550275730616, -0.082301927106886, -0.071799821619312, 0.428483476377619, 0.793777222625621,
            0.405176902409617, -0.0611233900026729, -0.0657719112818555, 0.0234526961418363, 0.00778259642732542, -0.00379351286449101
        })
            Yield New Wavelet("Coiflet_4", {
            -0.00000178498500308826, -0.00000325968023688337, 0.0000312298758653456, 0.0000623390344610071, -0.000259974552487713, -0.000589020756244338,
            0.00126656192929894, 0.00375143615727846, -0.00565828668661072, -0.0152117315279463, 0.0250822618448641, 0.0393344271233375,
            -0.096220442033988, -0.066627474263425, 0.434386056491469, 0.782238930920499, 0.41530840703043, -0.0560773133167548,
            -0.0812666996808788, 0.0266823001560531, 0.0160689439647763, -0.00734616632764209, -0.00162949201260173, 0.000892313668582315
        })
            Yield New Wavelet("Coiflet_5", {
            -0.0000000951765727381917, -0.00000016744288576823, 0.00000206376185136468, 0.0000037346551751414, -0.0000213150268099558, -0.0000413404322725125,
            0.000140541149702034, 0.000302259581813063, -0.000638131343045111, -0.00166286370201308, 0.00243337321265767, 0.00676418544805308,
            -0.00916423116248185, -0.0197617789425726, 0.0326835742671118, 0.0412892087501817, -0.105574208703339, -0.0620359639629036,
            0.437991626171837, 0.774289603652956, 0.421566206690851, -0.0520431631762438, -0.0919200105596962, 0.0281680289709364,
            0.0234081567858392, -0.0101311175198498, -0.00415935878138605, 0.00217823635810902, 0.000358589687895738, -0.000212080839803798
        })
        End Function
    End Class
End Namespace

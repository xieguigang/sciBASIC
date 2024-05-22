#Region "Microsoft.VisualBasic::a2afc9e74920c64f896d45122dc4d7d1, Data_science\Mathematica\SignalProcessing\SignalProcessing\WaveletTransform\Constructors\Symlets.vb"

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

    '   Total Lines: 38
    '    Code Lines: 35 (92.11%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 3 (7.89%)
    '     File Size: 3.19 KB


    '     Class Symlets
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateAllSymlets
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace WaveletTransform

    Public Class Symlets : Inherits WaveletConstructor

        Public Sub New()
            MyBase.New(wavelets:=CreateAllSymlets)
        End Sub

        Public Shared Iterator Function CreateAllSymlets() As IEnumerable(Of Wavelet)
            Yield New Wavelet("Symlet_2", {
            -0.129409522550921, 0.224143868041857, 0.836516303737469, 0.48296291314469
        })
            Yield New Wavelet("Symlet_3", {
                          0.0352262918821007, -0.0854412738822415, -0.135011020010391,
                          0.459877502119331, 0.806891509313339, 0.332670552950957
                          })
            Yield New Wavelet("Symlet_4", {-0.0757657147892733, -0.0296355276459985, 0.497618667632015, 0.803738751805916, 0.297857795605277, -0.0992195435768472,
            -0.0126039672620378, 0.0322231006040427})
            Yield New Wavelet("Symlet_5", {0.027333068345078, 0.0295194909257746, -0.0391342493023831, 0.199397533977394, 0.723407690402421, 0.633978963458212,
            0.0166021057645223, -0.17532808990845, -0.0211018340247589, 0.0195388827352867})
            Yield New Wavelet("Symlet_6", {0.0154041093270274, 0.00349071208421747, -0.117990111148191, -0.048311742585633, 0.491055941926747, 0.787641141030194,
            0.337929421727622, -0.0726375227864625, -0.0210602925123006, 0.0447249017706658, 0.0017677118642428, -0.00780070832503415})
            Yield New Wavelet("Symlet_7", {0.00268181456825788, -0.00104738488868292, -0.0126363034032519, 0.0305155131659636, 0.0678926935013727, -0.0495528349371273,
            0.0174412550868558, 0.536101917091763, 0.767764317003164, 0.288629631751515, -0.140047240442962, -0.107808237703818,
            0.00401024487153366, 0.0102681767085113})
            Yield New Wavelet("Symlet_8", {-0.00338241595100613, -0.000542132331791148, 0.031695087811493, 0.00760748732491761, -0.14329423835081, -0.0612733590676585,
            0.481359651258372, 0.777185751700524, 0.364441894835331, -0.051945838107709, -0.027219029917056, 0.0491371796736075,
            0.00380875201389062, -0.0149522583370482, -0.000302920514721367, 0.00188995033275946})
            Yield New Wavelet("Symlet_9", {0.00140091552591468, 0.000619780888985587, -0.0132719677818171, -0.0115282102076792, 0.0302248788582757, 0.000583462746125807,
            -0.0545689584308341, 0.238760914607303, 0.717897082764412, 0.617338449140936, 0.0352724880352719, -0.191550831297285,
            -0.018233770779396, 0.062077789302886, 0.00885926749340048, -0.0102640640276331, -0.000473154498680083, 0.00106949003290861})
            Yield New Wavelet("Symlet_10", {0.00077015980911449, 0.0000956326707228948, -0.00864129927702242, -0.00146538258130505, 0.0459272392310922, 0.0116098939037114,
            -0.159494278884918, -0.0708805357832439, 0.471690666938439, 0.769510037021107, 0.383826761067085, -0.0355367404738176,
            -0.0319900568824278, 0.0499949720773767, 0.00576491203358191, -0.0203549398123113, -0.000804358932016545, 0.00459317358531183,
            0.0000570360836184943, -0.000459329421004659})
        End Function
    End Class
End Namespace

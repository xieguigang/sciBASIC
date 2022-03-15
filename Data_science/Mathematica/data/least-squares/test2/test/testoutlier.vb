#Region "Microsoft.VisualBasic::4ba37d35a89a53e730191af6f9a56639, sciBASIC#\Data_science\Mathematica\data\least-squares\test2\test\testoutlier.vb"

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

    '   Total Lines: 46
    '    Code Lines: 24
    ' Comment Lines: 4
    '   Blank Lines: 18
    '     File Size: 1.56 KB


    ' Module testoutlier
    ' 
    '     Sub: Main, orderSeq
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.Bootstrapping.Outlier
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

Module testoutlier

    Sub Main()

        Call orderSeq()

        Dim x As Vector = {0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649}

        '  Dim index = x.OutlierIndex.ToArray


        x = {0, 0.010228592, 12.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649, 0.051605649, 0.091605649, 0.851605649, 2.55555, 0.051655649, 0.51655649, 0.8051655649, 1.00151655649, 3.9}

        '  index = x.OutlierIndex.ToArray

        '   Dim xy = index.RemovesOutlier(x.AsVector, x.AsVector)


        Call x.ToArray.GetJson(indent:=True).__DEBUG_ECHO
        ' Call xy.X.ToArray.GetJson(indent:=True).__DEBUG_ECHO

        Pause()

    End Sub

    Sub orderSeq()
        Dim x As Vector = {10, -3, 0.010228592, 2.278282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649, -10, 1}
        Dim Y As Vector = {9, -5, 10.010228592, 2.578282642, 0.922615588, 0.472233653, 0.234864831, 0.117762581, 0.051605649, -11, 1.2222}

        Dim index = x.OrderSequenceOutlierIndex.ToArray
        Dim line = x.Select(Function(p, i) New PointF(p, Y(i))).ToArray

        Dim xy = line.DeleteOutlier.ToArray

        Call xy.SaveTo("./line.csv")
        Call line.SaveTo("./line-raw.csv")

        Pause()
    End Sub
End Module

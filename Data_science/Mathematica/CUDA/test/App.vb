#Region "Microsoft.VisualBasic::b6299fedd32f6a4cd19f62f60c095189, sciBASIC#\Data_science\Mathematica\CUDA\test\App.vb"

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

    '   Total Lines: 20
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 578.00 B


    ' Module App
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math

Module App

    Public Declare Function GetPearson Lib "sciKernel.dll" Alias "pearson" (ByRef x As Double(), ByRef y As Double()) As Double

    Sub Main()
        Dim x As Double() = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10}
        Dim y As Double() = {10, 9, 8, 7, 6, 5, 4, 3, 2, 1}

        Dim pcc1 = GetPearson(x, y)
        Dim pcc2 = Correlations.GetPearson(x, y)

        Call Console.WriteLine($"pcc from rust => {pcc1}")
        Call Console.WriteLine($"pcc from vb.net => {pcc2}")

        Pause()
    End Sub

End Module

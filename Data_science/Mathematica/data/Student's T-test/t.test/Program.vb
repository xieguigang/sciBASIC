#Region "Microsoft.VisualBasic::13496174fc799effd7a6c1d0da389b9c, ..\sciBASIC#\Data_science\Mathematica\data\Student's T-test\t.test\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Mathematical.Statistics.Hypothesis
Imports Microsoft.VisualBasic.Serialization.JSON

Module Program

    Sub Main()
        Dim a#() = {175, 168, 168, 190, 156, 181, 182, 175, 174, 179}
        Dim b#() = {185, 169, 173, 173, 188, 186, 175, 174, 179, 180}

        With t.Test(a, b)
            Call $"alternative hypothesis: { .Valid}".__DEBUG_ECHO
            Call .GetJson(True).__DEBUG_ECHO
        End With

        Dim x#() = {0, 1, 1, 1}

        With t.Test(x, mu:=1)
            Call $"alternative hypothesis: { .Valid}".__DEBUG_ECHO
            Call .GetJson(True).__DEBUG_ECHO
        End With

        a = {1846523.253, 6840877.665, 2806323.704}
        b = {3056565.388, 1831431.105, 2933659.497}

        Call t.Test(a, b).GetJson.__DEBUG_ECHO
        Call t.Test(a, b, varEqual:=False).GetJson.__DEBUG_ECHO


        Pause()
    End Sub
End Module

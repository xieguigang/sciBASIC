#Region "Microsoft.VisualBasic::ee3b8173c99f9740085a7036018135ff, Data_science\Mathematica\Math\Math\DownSampling\EventOrder.vb"

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

    '   Total Lines: 45
    '    Code Lines: 36 (80.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (20.00%)
    '     File Size: 1.85 KB


    '     Module EventOrder
    ' 
    '         Function: BY_ABS_VAL_ASC, BY_ABS_VAL_DESC, BY_TIME_ASC, BY_VAL_ASC, BY_VAL_DESC
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports std = System.Math

Namespace DownSampling


    Public Module EventOrder

        Public Function BY_TIME_ASC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.time < e2.time, -1, 1)
        End Function

        Public Function BY_VAL_ASC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.intensity < e2.intensity, -1, 1)
        End Function

        Public Function BY_VAL_DESC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(e1.intensity < e2.intensity, 1, -1)
        End Function

        Public Function BY_ABS_VAL_ASC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(std.Abs(e1.intensity) < std.Abs(e2.intensity), -1, 1)
        End Function

        Public Function BY_ABS_VAL_DESC(e1 As ITimeSignal, e2 As ITimeSignal) As Integer
            If (e1 Is Nothing AndAlso e2 Is Nothing) Then Return 0
            If (e1 Is Nothing) Then Return -1
            If (e2 Is Nothing) Then Return 1
            Return If(std.Abs(e1.intensity) < std.Abs(e2.intensity), 1, -1)
        End Function
    End Module

End Namespace

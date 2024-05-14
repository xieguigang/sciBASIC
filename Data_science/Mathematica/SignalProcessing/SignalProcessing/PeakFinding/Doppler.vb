#Region "Microsoft.VisualBasic::71a5a884164a96138371dd378dc27abb, Data_science\Mathematica\SignalProcessing\SignalProcessing\PeakFinding\Doppler.vb"

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

    '   Total Lines: 31
    '    Code Lines: 24
    ' Comment Lines: 3
    '   Blank Lines: 4
    '     File Size: 1.11 KB


    ' Module Doppler
    ' 
    '     Function: Calculate
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math.SignalProcessing.PeakFinding
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Public Module Doppler

    Public Function Calculate(signals As IEnumerable(Of ITimeSignal))
        ' 首先平移到最低点
        Dim raw = signals.ToArray
        Dim into = raw.Min(Function(x) x.intensity)

        If into < 0 Then
            into = stdNum.Abs(into)
            raw = raw _
                .Select(Function(a)
                            Return New TimeSignal With {
                                .time = a.time,
                                .intensity = a.intensity + into
                            }
                        End Function) _
                .As(Of ITimeSignal) _
                .ToArray
        End If

        ' 查找出所有的peaks
        Dim allPeaks = New ElevationAlgorithm(5, 0.65).FindAllSignalPeaks(raw).ToArray
        ' 计算出频率的变化趋势
        Throw New NotImplementedException
    End Function
End Module

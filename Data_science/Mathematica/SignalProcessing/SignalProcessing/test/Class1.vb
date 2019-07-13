#Region "Microsoft.VisualBasic::9d169e36ad388ba26b791510368051fe, Data_science\Mathematica\SignalProcessing\SignalProcessing\test\Class1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.Wave
Imports Microsoft.VisualBasic.Data.IO


Module Module1

    Sub Main()

        Dim wav = "E:\VB_GamePads\runtime\sciBASIC#\Data_science\Mathematica\SignalProcessing\wav\M1F1-int16-AFsp.wav".OpenBinaryReader
        Dim header As File = File.Open(wav)

        '    Dim chunks = header.data.data.Split(1024)

        Pause()
    End Sub

End Module

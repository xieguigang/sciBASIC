#Region "Microsoft.VisualBasic::1b632f9926babbc7c55c6f6f1a48f9fe, Data_science\Mathematica\SignalProcessing\SignalProcessing\COW\Flags.vb"

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
    '    Code Lines: 14 (70.00%)
    ' Comment Lines: 4 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (10.00%)
    '     File Size: 423 B


    '     Enum TraceDirection
    ' 
    '         Ali, Hor, Ver
    ' 
    '  
    ' 
    ' 
    ' 
    '     Enum BorderLimit
    ' 
    '         Constant, Diamond, Gaussian, Linear, Quad
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace COW

    ''' <summary>
    ''' This class is not used in MS-DIAL and MS-FINDER programs.
    ''' This class is now being used in MRM-PROBS and MRM-DIFF programs.
    ''' </summary>
    Public Enum TraceDirection
        Ali
        Ver
        Hor
    End Enum

    Public Enum BorderLimit
        Constant
        Linear
        Quad
        Diamond
        Gaussian
    End Enum
End Namespace

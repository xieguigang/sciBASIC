#Region "Microsoft.VisualBasic::b1ad0481a4e44c0fecc059e882448bf7, Data_science\Mathematica\Math\Math\Numerics\Optimization\LBFGSB\Parameters.vb"

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

    '   Total Lines: 29
    '    Code Lines: 24 (82.76%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (17.24%)
    '     File Size: 935 B


    '     Class Parameters
    ' 
    ' 
    ' 
    '     Enum LINESEARCH
    ' 
    '         LEWISOVERTON, MORETHUENTE_LBFGSPP, MORETHUENTE_ORIG
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Framework.Optimization.LBFGSB

    Public NotInheritable Class Parameters

        Public m As Integer = 6
        Public epsilon As Double = 0.0000001
        Public epsilon_rel As Double = 0.0000001
        Public past As Integer = 3
        Public delta As Double = 0.0000000001
        Public max_iterations As Integer = 1000
        Public max_submin As Integer = 10
        Public max_linesearch As Integer = 20
        Public linesearch As LINESEARCH = LINESEARCH.MORETHUENTE_ORIG
        Public xtol As Double = 0.00000001 ' MoreThuente
        Public min_step As Double = 1.0E-20
        Public max_step As Double = 1.0E+20
        Public ftol As Double = 0.0001
        Public wolfe As Double = 0.9
        Public weak_wolfe As Boolean = True

    End Class

    Public Enum LINESEARCH
        MORETHUENTE_LBFGSPP
        MORETHUENTE_ORIG
        LEWISOVERTON
    End Enum

End Namespace

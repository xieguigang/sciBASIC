#Region "Microsoft.VisualBasic::2dc62d5250c1d80f367e3a33d16e3bff, Data_science\Mathematica\Math\DataFittings\Levenberg-Marquardt\LmParamHandler.vb"

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

    '   Total Lines: 13
    '    Code Lines: 5 (38.46%)
    ' Comment Lines: 7 (53.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (7.69%)
    '     File Size: 450 B


    '     Interface LmParamHandler
    ' 
    '         Sub: adjust
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace LevenbergMarquardt
    ''' <summary>
    ''' Created by duy on 18/3/15.
    ''' </summary>
    Public Interface LmParamHandler
        ''' <summary>
        ''' Adjusts (or modifies) values of the Levenberg-Marquardt parameters
        ''' </summary>
        ''' <param name="lmParams"> Numbers which are values of Levenberg-Marquardt parameters </param>
        Sub adjust(lmParams As Double())
    End Interface

End Namespace

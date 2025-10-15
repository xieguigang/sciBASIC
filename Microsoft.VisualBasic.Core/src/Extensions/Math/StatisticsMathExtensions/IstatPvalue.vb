#Region "Microsoft.VisualBasic::574a83a379d7fd501a8d4d57732dd0c9, Microsoft.VisualBasic.Core\src\Extensions\Math\StatisticsMathExtensions\IstatPvalue.vb"

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

    '   Total Lines: 16
    '    Code Lines: 5 (31.25%)
    ' Comment Lines: 8 (50.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (18.75%)
    '     File Size: 427 B


    '     Interface IStatPvalue
    ' 
    '         Properties: pValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Math.Statistics

    ''' <summary>
    ''' a general abstract object model for statistics result which 
    ''' contains p value information
    ''' </summary>
    Public Interface IStatPvalue

        ''' <summary>
        ''' p value of current sample statistics result
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property pValue As Double

    End Interface
End Namespace

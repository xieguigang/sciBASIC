#Region "Microsoft.VisualBasic::84796e74042cc8fafd8687b2a91ee6a0, Microsoft.VisualBasic.Core\src\Extensions\Math\StatisticsMathExtensions\IstatPvalue.vb"

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

    '   Total Lines: 22
    '    Code Lines: 8 (36.36%)
    ' Comment Lines: 8 (36.36%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (27.27%)
    '     File Size: 542 B


    '     Interface IStatPvalue
    ' 
    '         Properties: pValue
    ' 
    '     Interface IStatFDR
    ' 
    '         Properties: adjPVal
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

    Public Interface IStatFDR : Inherits IStatPvalue

        Property adjPVal As Double

    End Interface
End Namespace

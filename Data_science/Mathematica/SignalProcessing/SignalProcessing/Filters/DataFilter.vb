#Region "Microsoft.VisualBasic::2fea55a73a2bdf7d04417b2f29b0e677, Data_science\Mathematica\SignalProcessing\SignalProcessing\Filters\DataFilter.vb"

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

    '   Total Lines: 14
    '    Code Lines: 5 (35.71%)
    ' Comment Lines: 6 (42.86%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 3 (21.43%)
    '     File Size: 352 B


    '     Interface DataFilter
    ' 
    '         Function: filter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Filters
    ''' <summary>
    ''' This interface represents types which are able to filter data, for example:
    ''' eliminate redundant points.
    ''' 
    ''' @author Marcin Rzeźnicki </summary>
    ''' 
    Public Interface DataFilter

        Function filter(data As Double()) As Double()

    End Interface

End Namespace

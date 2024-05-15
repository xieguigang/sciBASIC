#Region "Microsoft.VisualBasic::0f4f243e2219f3abcbf18d1c425a64b1, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\Enumerable\ISequenceData.vb"

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

    '   Total Lines: 21
    '    Code Lines: 5
    ' Comment Lines: 13
    '   Blank Lines: 3
    '     File Size: 714 B


    '     Interface ISequenceData
    ' 
    '         Properties: SequenceData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Collection.Generic

    ''' <summary>
    ''' The sequence analysis data model, example as biological sequence, 
    ''' time signal, time sequence anaysis
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks>
    ''' A general abstract model apply for the SGT algorithm analysis
    ''' </remarks>
    Public Interface ISequenceData(Of T, List As IEnumerable(Of T))

        ''' <summary>
        ''' the sequence data provider, example like time serial signals, 
        ''' logging data, biological sequence
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property SequenceData As List

    End Interface
End Namespace

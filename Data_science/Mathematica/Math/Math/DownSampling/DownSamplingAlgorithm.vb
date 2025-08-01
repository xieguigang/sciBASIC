#Region "Microsoft.VisualBasic::6605635f87cbf85b858bf3172e7a20b8, Data_science\Mathematica\Math\Math\DownSampling\DownSamplingAlgorithm.vb"

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

    '   Total Lines: 18
    '    Code Lines: 6 (33.33%)
    ' Comment Lines: 7 (38.89%)
    '    - Xml Docs: 85.71%
    ' 
    '   Blank Lines: 5 (27.78%)
    '     File Size: 567 B


    '     Interface DownSamplingAlgorithm
    ' 
    '         Function: process
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.TagData

Namespace DownSampling

    ''' <summary>
    ''' Interface for downSampling algorithms
    ''' </summary>
    Public Interface DownSamplingAlgorithm

        ''' 
        ''' <param name="data"> The original data </param>
        ''' <param name="threshold"> Number of data points to be returned </param>
        ''' <returns> the downsampled data </returns>
        Function process(data As IList(Of ITimeSignal), threshold As Integer) As IList(Of ITimeSignal)

    End Interface

End Namespace

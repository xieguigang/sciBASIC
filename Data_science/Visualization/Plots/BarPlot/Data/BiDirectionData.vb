#Region "Microsoft.VisualBasic::d44d9532ee8f6421bde927aa40625310, Data_science\Visualization\Plots\BarPlot\Data\BiDirectionData.vb"

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

    '   Total Lines: 34
    '    Code Lines: 17 (50.00%)
    ' Comment Lines: 12 (35.29%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (14.71%)
    '     File Size: 859 B


    '     Class BiDirectionData
    ' 
    '         Properties: Factor1, Factor2, samples, size
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace BarPlot.Data

    Public Class BiDirectionData

        ''' <summary>
        ''' left
        ''' </summary>
        ''' <returns></returns>
        Public Property Factor1 As String
        ''' <summary>
        ''' right
        ''' </summary>
        ''' <returns></returns>
        Public Property Factor2 As String
        ''' <summary>
        ''' data samples
        ''' </summary>
        ''' <returns></returns>
        Public Property samples As BarDataSample()

        Public ReadOnly Property size As Integer
            Get
                Return samples.Length
            End Get
        End Property

        Default Public ReadOnly Property data(i As Integer) As BarDataSample
            Get
                Return samples(i)
            End Get
        End Property

    End Class
End Namespace

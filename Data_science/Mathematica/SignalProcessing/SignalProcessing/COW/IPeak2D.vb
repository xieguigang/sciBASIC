#Region "Microsoft.VisualBasic::6affdbf7ce5fb2225912d13837aded85, Data_science\Mathematica\SignalProcessing\SignalProcessing\COW\IPeak2D.vb"

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

    '   Total Lines: 44
    '    Code Lines: 9 (20.45%)
    ' Comment Lines: 30 (68.18%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (11.36%)
    '     File Size: 1.54 KB


    '     Interface IPeak2D
    ' 
    '         Properties: Dimension1, Dimension2, ID, Intensity
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace COW

    Public Interface IPeak2D

        ''' <summary>
        ''' the unique reference id of current signal peak
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property ID As String
        ''' <summary>
        ''' the signal peak dimension 1 data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' in LCMS signal processing, usually could be the [Mass] data.
        ''' </remarks>
        ReadOnly Property Dimension1 As Double
        ''' <summary>
        ''' the signal peak dimension 2 data
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' in LCMS signal processing, usually could be the [RT] time data.
        ''' </remarks>
        ReadOnly Property Dimension2 As Double
        ''' <summary>
        ''' the signal intensity value
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Intensity As Double

    End Interface

    ''' <summary>
    ''' Function to create a signal peak object
    ''' </summary>
    ''' <param name="id"><see cref="IPeak2D.ID"/></param>
    ''' <param name="dim1"><see cref="IPeak2D.Dimension1"/></param>
    ''' <param name="dim2"><see cref="IPeak2D.Dimension2"/></param>
    ''' <param name="intensity"><see cref="IPeak2D.Intensity"/></param>
    ''' <returns></returns>
    Public Delegate Function IDelegateCreatePeak2D(Of T)(id As String, dim1 As Double, dim2 As Double, intensity As Double) As T

End Namespace

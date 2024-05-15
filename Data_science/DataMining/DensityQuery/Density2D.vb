#Region "Microsoft.VisualBasic::0a6eae31f56e9b9e10a580dfa4841081, Data_science\DataMining\DensityQuery\Density2D.vb"

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

    '   Total Lines: 73
    '    Code Lines: 49
    ' Comment Lines: 13
    '   Blank Lines: 11
    '     File Size: 2.90 KB


    ' Module Density2D
    ' 
    '     Function: (+2 Overloads) Density, WindowSize
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph

Public Module Density2D

    <Extension>
    Public Function WindowSize(Of T)(grid As Grid(Of T), w As Integer, h As Integer) As GridBox(Of T)
        Return New GridBox(Of T)(grid, w, h)
    End Function

    <Extension>
    Public Function Density(Of T As INamedValue)(data As IEnumerable(Of T),
                                                 getX As Func(Of T, Integer),
                                                 getY As Func(Of T, Integer),
                                                 gridSize As Size) As IEnumerable(Of NamedValue(Of Double))

        Return data.Density(Function(d) d.Key, getX, getY, gridSize)
    End Function

    ''' <summary>
    ''' get density value of geometry points
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="getName"></param>
    ''' <param name="getX"></param>
    ''' <param name="getY"></param>
    ''' <param name="gridSize"></param>
    ''' <param name="parallel"></param>
    ''' <returns>
    ''' the resulted density value is in range of ``[0,1]``.
    ''' </returns>
    <Extension>
    Public Iterator Function Density(Of T)(data As IEnumerable(Of T),
                                           getName As Func(Of T, String),
                                           getX As Func(Of T, Integer),
                                           getY As Func(Of T, Integer),
                                           gridSize As Size,
                                           Optional parallel As Boolean = False) As IEnumerable(Of NamedValue(Of Double))

        Dim grid2 As Grid(Of T) = Grid(Of T).Create(data, getX, getY)
        Dim A As Double = (gridSize.Width * 2) * (gridSize.Height * 2)

        If parallel Then
            For Each x As NamedValue(Of Double) In grid2 _
                .EnumerateData _
                .AsParallel _
                .Select(Function(xd)
                            Dim n = grid2.Query(getX(xd), getY(xd), gridSize).Count
                            Dim dd = n / A
                            Dim i As New NamedValue(Of Double)(getName(xd), dd)

                            Return i
                        End Function)

                Yield x
            Next
        Else
            Dim q As T()
            Dim d As Double

            For Each x As T In grid2.EnumerateData
                q = grid2.Query(getX(x), getY(x), gridSize).ToArray
                d = q.Length / A

                Yield New NamedValue(Of Double)(getName(x), d)
            Next
        End If
    End Function
End Module

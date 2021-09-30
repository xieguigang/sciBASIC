Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory

Public Module Density2D

    <Extension>
    Public Function Density(Of T As INamedValue)(data As IEnumerable(Of T),
                                                 getX As Func(Of T, Integer),
                                                 getY As Func(Of T, Integer),
                                                 gridSize As Size) As IEnumerable(Of NamedValue(Of Double))

        Return data.Density(Function(d) d.Key, getX, getY, gridSize)
    End Function

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

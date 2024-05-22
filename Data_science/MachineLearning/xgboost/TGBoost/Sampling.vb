#Region "Microsoft.VisualBasic::b28f96d34a7cd0de5a99bea569856d95, Data_science\MachineLearning\xgboost\TGBoost\Sampling.vb"

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

    '   Total Lines: 46
    '    Code Lines: 35 (76.09%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 11 (23.91%)
    '     File Size: 1.30 KB


    '     Class RowSampler
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: shuffle
    ' 
    '     Class ColumnSampler
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: shuffle
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Java.Arrays
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace train
    Public Class RowSampler

        Public row_mask As New List(Of Double)()

        Public Sub New(n As Integer, sampling_rate As Double)
            For i = 0 To n - 1
                row_mask.Add(If(randf.NextDouble <= sampling_rate, 1.0, 0.0))
            Next
        End Sub

        Public Overridable Sub shuffle()
            Dim rands = row_mask.Shuffles

            row_mask.Clear()
            row_mask.AddRange(rands)
        End Sub
    End Class

    Public Class ColumnSampler

        Private cols As New List(Of Integer)()
        Public col_selected As List(Of Integer)
        Private n_selected As Integer

        Public Sub New(n As Integer, sampling_rate As Double)
            For i = 0 To n - 1
                cols.Add(i)
            Next

            n_selected = CInt(n * sampling_rate)
            col_selected = cols.subList(0, n_selected)
        End Sub

        Public Overridable Sub shuffle()
            Dim rands = cols.Shuffles

            cols.Clear()
            cols.AddRange(rands)
            col_selected = cols.subList(0, n_selected)
        End Sub
    End Class
End Namespace

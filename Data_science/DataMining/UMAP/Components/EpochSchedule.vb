#Region "Microsoft.VisualBasic::b8a43613a97f5b1ff9fcb133adecb59e, Data_science\DataMining\UMAP\Components\EpochSchedule.vb"

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

    '   Total Lines: 103
    '    Code Lines: 42 (40.78%)
    ' Comment Lines: 49 (47.57%)
    '    - Xml Docs: 95.92%
    ' 
    '   Blank Lines: 12 (11.65%)
    '     File Size: 3.59 KB


    ' Class EpochSchedule
    ' 
    '     Properties: [Default], Epochs, Thresholds
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: GetEpochs, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

''' <summary>
''' The number-of-epochs heuristic that is used by the SGD optimization.
''' </summary>
''' <remarks>
''' This heuristic differs from the python version of the UMAP implementation,
''' the default configuration of this class keeps the original behaviour of 
''' this VB.NET implementation:
''' 
''' ```
''' n &lt;= 2500  -> 500
''' n &lt;= 5000  -> 400
''' n &lt;= 7500  -> 300
''' n &gt;  7500  -> 200
''' ```
''' </remarks>
Public Class EpochSchedule

    ''' <summary>
    ''' the upper bound of the dataset size, should be an ascending 
    ''' ordered integer vector
    ''' </summary>
    ''' <returns></returns>
    Public Property Thresholds As Integer()
    ''' <summary>
    ''' the epochs of the corresponding threshold slot, the length of 
    ''' this vector should be equals to the length of the 
    ''' <see cref="Thresholds"/> plus one.
    ''' </summary>
    ''' <returns></returns>
    Public Property Epochs As Integer()

    ''' <summary>
    ''' the default schedule configuration
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property [Default] As EpochSchedule =
        New EpochSchedule With {
            .Thresholds = {2500, 5000, 7500},
            .Epochs = {500, 400, 300, 200}
        }

    Sub New()
        Call Me.New({2500, 5000, 7500}, {500, 400, 300, 200})
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="thresholds">
    ''' the upper bound of the dataset size, should be an ascending 
    ''' ordered integer vector
    ''' </param>
    ''' <param name="epochs">
    ''' the epochs of the corresponding threshold slot, the length of 
    ''' this vector should be equals to the length of the 
    ''' <paramref name="thresholds"/> plus one.
    ''' </param>
    Sub New(thresholds As Integer(), epochs As Integer())
        If thresholds Is Nothing OrElse thresholds.Length = 0 Then
            Throw New ArgumentNullException(NameOf(thresholds))
        End If
        If epochs Is Nothing OrElse epochs.Length <> thresholds.Length + 1 Then
            Throw New ArgumentException($"the size of the {NameOf(epochs)} vector should be equals to the size of the {NameOf(thresholds)} vector plus one!", NameOf(epochs))
        End If
        If epochs.Any(Function(n) n <= 0) Then
            Throw New ArgumentOutOfRangeException(NameOf(epochs), "all of the epoch values should be a positive integer!")
        End If

        Thresholds = CType(thresholds.Clone, Integer())
        Epochs = CType(epochs.Clone, Integer())
    End Sub

    ''' <summary>
    ''' evaluate the number of epochs for optimize the projection
    ''' </summary>
    ''' <param name="length">
    ''' the number of the samples inside the dataset
    ''' </param>
    ''' <returns></returns>
    Public Function GetEpochs(length As Integer) As Integer
        For i As Integer = 0 To Thresholds.Length - 1
            If length <= Thresholds(i) Then
                Return Epochs(i)
            End If
        Next

        Return Epochs(Thresholds.Length)
    End Function

    Public Overrides Function ToString() As String
        Dim slots As String() = New String(Thresholds.Length - 1) {}

        For i As Integer = 0 To Thresholds.Length - 1
            slots(i) = $"n<={Thresholds(i)}:{Epochs(i)}"
        Next

        Return $"{{{String.Join(", ", slots)}, else:{Epochs(Thresholds.Length)}}}"
    End Function

End Class


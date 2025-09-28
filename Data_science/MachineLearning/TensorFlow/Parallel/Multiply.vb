#Region "Microsoft.VisualBasic::316a67159d297f302f4878ec66ccfc68, Data_science\MachineLearning\TensorFlow\Parallel\Multiply.vb"

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

    '   Total Lines: 45
    '    Code Lines: 30 (66.67%)
    ' Comment Lines: 6 (13.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (20.00%)
    '     File Size: 1.35 KB


    '     Class MultiplyScale
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Scale
    ' 
    '         Sub: Solve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Parallel

Namespace Parallel

    Public Class MultiplyScale : Inherits VectorTask

        ReadOnly T As Tensor
        ReadOnly s As Double

        Sub New(tensor As Tensor, scale As Double)
            Call MyBase.New(tensor.nrValues)

            T = New Tensor(tensor, copy:=True)
            s = scale
        End Sub

        Protected Overrides Sub Solve(start As Integer, ends As Integer, cpu_id As Integer)
            For c As Integer = start To ends
                T.values(c) = T.values(c) * s
            Next
        End Sub

        ''' <summary>
        ''' Scale all elements with a number
        ''' </summary>
        ''' <param name="tensor"></param>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Function Scale(tensor As Tensor, s As Double) As Tensor
            If tensor.nrValues < 1000 Then
                Dim T As New Tensor(tensor, True)

                For c As Integer = 0 To tensor.nrValues - 1
                    T.values(c) = tensor.values(c) * s
                Next

                Return T
            Else
                Dim task As New MultiplyScale(tensor, s)
                Call task.Run()
                Return task.T
            End If
        End Function
    End Class
End Namespace

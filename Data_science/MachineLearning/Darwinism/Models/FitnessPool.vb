#Region "Microsoft.VisualBasic::96dbd90f8e16809fa949a179af621bc5, Data_science\MachineLearning\Darwinism\Models\FitnessPool.vb"

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

    '     Class FitnessPool
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Fitness
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Darwinism.Models

    Public Class FitnessPool(Of Individual)

        Protected Friend ReadOnly cache As New Dictionary(Of String, Double)
        Protected caclFitness As Func(Of Individual, Double)

        Sub New(cacl As Func(Of Individual, Double))
            caclFitness = cacl
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' This function tells how well given individual performs at given problem.
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        Public Function Fitness([in] As Individual) As Double
            Dim key$ = [in].ToString
            Dim fit As Double

            SyncLock cache
                If cache.ContainsKey(key$) Then
                    fit = cache(key$)
                Else
                    fit = caclFitness([in])
                    cache.Add(key$, fit)
                End If
            End SyncLock

            Return fit
        End Function
    End Class
End Namespace

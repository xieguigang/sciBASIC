#Region "Microsoft.VisualBasic::b4135dc8e62a3b7837559f051b0884a9, ..\sciBASIC#\Data_science\MachineLearning\Darwinism\Models\FitnessPool.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Darwinism.Models

    Public Class FitnessPool(Of Individual, T As IComparable(Of T))

        Protected Friend ReadOnly cache As New Dictionary(Of String, T)
        Protected caclFitness As Func(Of Individual, T)

        Sub New(cacl As Func(Of Individual, T))
            caclFitness = cacl
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' This function tells how well given individual performs at given problem.
        ''' </summary>
        ''' <param name="[in]"></param>
        ''' <returns></returns>
        Public Function Fitness([in] As Individual) As T
            Dim key$ = [in].ToString
            Dim fit As T

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

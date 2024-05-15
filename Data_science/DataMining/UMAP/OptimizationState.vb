#Region "Microsoft.VisualBasic::97a10524f5b8cef92ed4eb15b9ca6b30, Data_science\DataMining\UMAP\OptimizationState.vb"

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

    '   Total Lines: 39
    '    Code Lines: 23
    ' Comment Lines: 12
    '   Blank Lines: 4
    '     File Size: 1.36 KB


    ' Class OptimizationState
    ' 
    '     Function: GetDistanceFactor
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Friend NotInheritable Class OptimizationState

    Public CurrentEpoch As Integer = 0
    ''' <summary>
    ''' the index of source node
    ''' </summary>
    Public Head As Integer() = New Integer(-1) {}
    ''' <summary>
    ''' the index of target node
    ''' </summary>
    Public Tail As Integer() = New Integer(-1) {}
    Public EpochsPerSample As Double() = New Double(-1) {}
    ''' <summary>
    ''' edge weight?
    ''' </summary>
    Public EpochOfNextSample As Double() = New Double(-1) {}
    Public EpochOfNextNegativeSample As Double() = New Double(-1) {}
    Public EpochsPerNegativeSample As Double() = New Double(-1) {}
    Public MoveOther As Boolean = True
    Public InitialAlpha As Double = 1
    Public Alpha As Double = 1
    Public Gamma As Double = 1
    Public A As Double = 1.57694352F
    Public B As Double = 0.8950609F

    ''' <summary>
    ''' the dimension result of the projection operation
    ''' </summary>
    Public [Dim] As Integer = 2
    Public NEpochs As Integer = 500
    Public NVertices As Integer = 0

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDistanceFactor(distSquared As Double) As Double
        Return 1.0F / ((0.001F + distSquared) * CSng(A * (distSquared ^ B) + 1))
    End Function
End Class

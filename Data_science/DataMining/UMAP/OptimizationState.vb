#Region "Microsoft.VisualBasic::7d6b8b7aa45daf323a0b67ddfb3ccdb8, Data_science\DataMining\UMAP\OptimizationState.vb"

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

    ' Class OptimizationState
    ' 
    '     Function: GetDistanceFactor
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Friend NotInheritable Class OptimizationState

    Public CurrentEpoch As Integer = 0
    Public Head As Integer() = New Integer(-1) {}
    Public Tail As Integer() = New Integer(-1) {}
    Public EpochsPerSample As Single() = New Single(-1) {}
    Public EpochOfNextSample As Single() = New Single(-1) {}
    Public EpochOfNextNegativeSample As Single() = New Single(-1) {}
    Public EpochsPerNegativeSample As Single() = New Single(-1) {}
    Public MoveOther As Boolean = True
    Public InitialAlpha As Single = 1
    Public Alpha As Single = 1
    Public Gamma As Single = 1
    Public A As Single = 1.57694352F
    Public B As Single = 0.8950609F
    Public [Dim] As Integer = 2
    Public NEpochs As Integer = 500
    Public NVertices As Integer = 0

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDistanceFactor(distSquared As Single) As Single
        Return 1.0F / ((0.001F + distSquared) * CSng(A * (distSquared ^ B) + 1))
    End Function
End Class

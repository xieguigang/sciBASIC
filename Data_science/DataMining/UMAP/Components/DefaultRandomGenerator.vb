#Region "Microsoft.VisualBasic::f2724ddaa591ec0d6492bdca0e7e9fc4, Data_science\DataMining\UMAP\Components\DefaultRandomGenerator.vb"

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

    '   Total Lines: 38
    '    Code Lines: 23 (60.53%)
    ' Comment Lines: 7 (18.42%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (21.05%)
    '     File Size: 1.79 KB


    ' Class DefaultRandomGenerator
    ' 
    '     Properties: DisableThreading, Instance, IsThreadSafe
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: [Next], NextFloat
    ' 
    '     Sub: NextFloats
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math

Public NotInheritable Class DefaultRandomGenerator
    Implements IProvideRandomValues

    ''' <summary>
    ''' This is the default configuration (it supports the optimization process to be executed on multiple threads)
    ''' </summary>
    Public Shared ReadOnly Property Instance As DefaultRandomGenerator = New DefaultRandomGenerator(allowParallel:=True)

    ''' <summary>
    ''' This uses the same random number generator but forces the optimization process to run on a single thread (which may be desirable if multiple requests may be processed concurrently
    ''' or if it is otherwise not desirable to let a single request access all of the CPUs)
    ''' </summary>
    Public Shared ReadOnly Property DisableThreading As DefaultRandomGenerator = New DefaultRandomGenerator(allowParallel:=False)

    Private Sub New(allowParallel As Boolean)
        IsThreadSafe = allowParallel
    End Sub

    Public ReadOnly Property IsThreadSafe As Boolean Implements IProvideRandomValues.IsThreadSafe

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [Next](minValue As Integer, maxValue As Integer) As Integer Implements IProvideRandomValues.Next
        Return ThreadSafeFastRandom.Next(minValue, maxValue)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function NextFloat() As Double Implements IProvideRandomValues.NextFloat
        Return ThreadSafeFastRandom.NextFloat()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub NextFloats(buffer As Double()) Implements IProvideRandomValues.NextFloats
        ThreadSafeFastRandom.NextFloats(buffer)
    End Sub
End Class

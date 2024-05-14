#Region "Microsoft.VisualBasic::62ba84d57fabde09150e5ee2c05dac64, Data_science\Mathematica\Math\Math\Numerics\Framework\Iterator.vb"

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

    '   Total Lines: 43
    '    Code Lines: 27
    ' Comment Lines: 7
    '   Blank Lines: 9
    '     File Size: 1.28 KB


    '     Module Iterator
    ' 
    '         Sub: Run
    '         Class Kernel
    ' 
    '             Properties: terminated
    ' 
    '             Function: Run
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language

Namespace Framework

    Public Module Iterator

        ''' <summary>
        ''' the simulator iterator kernel
        ''' </summary>
        Public MustInherit Class Kernel : Implements ITaskDriver

            Friend ReadOnly Property terminated As Boolean
                Get
                    Return is_terminated
                End Get
            End Property

            Protected is_terminated As Boolean

            ''' <summary>
            ''' Execute the iterator step.
            ''' </summary>
            ''' <param name="itr"></param>
            Protected Friend MustOverride Sub [Step](itr As Integer)

            Public Overridable Function Run() As Integer Implements ITaskDriver.Run
                Call Iterator.Run(Me)
                Return 0
            End Function
        End Class

        <Extension>
        Public Sub Run(kernel As Kernel, Optional iterations% = 10 * 10000)
            Dim i As i32 = 0

            Do While ++i <= iterations AndAlso Not kernel.terminated
                Call kernel.Step(itr:=i)
            Loop
        End Sub
    End Module
End Namespace

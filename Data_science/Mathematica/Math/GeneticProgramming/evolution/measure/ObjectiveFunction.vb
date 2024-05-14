#Region "Microsoft.VisualBasic::08d7c67bb4818dd5cdef6399e19a3d4c, Data_science\Mathematica\Math\GeneticProgramming\evolution\measure\ObjectiveFunction.vb"

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

    '   Total Lines: 65
    '    Code Lines: 50
    ' Comment Lines: 0
    '   Blank Lines: 15
    '     File Size: 2.30 KB


    '     Class ObjectiveFunction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Enum InnerEnum
    ' 
    '             MAE, MSE, SAE, SSE
    ' 
    ' 
    ' 
    '  
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ordinal, ToString, valueOf, values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace evolution.measure
    Public NotInheritable Class ObjectiveFunction

        Public Shared ReadOnly MAE As ObjectiveFunction = New ObjectiveFunction("MAE", InnerEnum.MAE, New MeanAbsoluteError())
        Public Shared ReadOnly MSE As ObjectiveFunction = New ObjectiveFunction("MSE", InnerEnum.MSE, New MeanSquareError())
        Public Shared ReadOnly SAE As ObjectiveFunction = New ObjectiveFunction("SAE", InnerEnum.SAE, New SumAbsoluteError())
        Public Shared ReadOnly SSE As ObjectiveFunction = New ObjectiveFunction("SSE", InnerEnum.SSE, New SumSquareError())

        Private Shared ReadOnly valueList As IList(Of ObjectiveFunction) = New List(Of ObjectiveFunction)()

        Shared Sub New()
            valueList.Add(MAE)
            valueList.Add(MSE)
            valueList.Add(SAE)
            valueList.Add(SSE)
        End Sub

        Public Enum InnerEnum
            MAE
            MSE
            SAE
            SSE
        End Enum

        Public ReadOnly innerEnumValue As InnerEnum
        Private ReadOnly nameValue As String
        Private ReadOnly ordinalValue As Integer
        Private Shared nextOrdinal As Integer = 0

        Public ReadOnly objective As Objective

        Private Sub New(name As String, innerEnum As InnerEnum, objective As Objective)
            Me.objective = objective

            nameValue = name
            ordinalValue = std.Min(Threading.Interlocked.Increment(nextOrdinal), nextOrdinal - 1)
            innerEnumValue = innerEnum
        End Sub


        Public Shared Function values() As IList(Of ObjectiveFunction)
            Return valueList
        End Function

        Public Function ordinal() As Integer
            Return ordinalValue
        End Function

        Public Overrides Function ToString() As String
            Return nameValue
        End Function

        Public Shared Function valueOf(name As String) As ObjectiveFunction
            For Each enumInstance In valueList
                If Equals(enumInstance.nameValue, name) Then
                    Return enumInstance
                End If
            Next
            Throw New ArgumentException(name)
        End Function
    End Class

End Namespace

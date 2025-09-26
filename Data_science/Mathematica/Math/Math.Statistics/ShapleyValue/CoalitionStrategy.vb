#Region "Microsoft.VisualBasic::1dec163e5968ffbd408f0cea3e1c7b4a, Data_science\Mathematica\Math\Math.Statistics\ShapleyValue\CoalitionStrategy.vb"

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

    '   Total Lines: 71
    '    Code Lines: 48 (67.61%)
    ' Comment Lines: 8 (11.27%)
    '    - Xml Docs: 62.50%
    ' 
    '   Blank Lines: 15 (21.13%)
    '     File Size: 2.09 KB


    '     Class CoalitionStrategy
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Enum InnerEnum
    ' 
    '             RANDOM, SEQUENTIAL
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: Sequential
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ordinal, ToString, valueOf, values
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace ShapleyValue

    ''' 
    ''' <summary>
    ''' @author Franck Benault
    ''' 
    ''' @version	0.0.2
    ''' @since 0.0.2
    ''' 
    ''' </summary>
    Public NotInheritable Class CoalitionStrategy

        Public Shared ReadOnly SEQUENTIAL_STRATEGY As New CoalitionStrategy("SEQUENTIAL", InnerEnum.SEQUENTIAL)

        Public Shared ReadOnly RANDOM_STRATEGY As New CoalitionStrategy("RANDOM", InnerEnum.RANDOM)

        Private Shared ReadOnly valueList As New List(Of CoalitionStrategy)()

        Shared Sub New()
            valueList.Add(SEQUENTIAL_STRATEGY)
            valueList.Add(RANDOM_STRATEGY)
        End Sub

        Public Enum InnerEnum
            SEQUENTIAL
            RANDOM
        End Enum

        Public ReadOnly innerEnumValue As InnerEnum
        Private ReadOnly nameValue As String
        Private ReadOnly ordinalValue As Integer
        Private Shared nextOrdinal As Integer = 0

        Private Sub New(name As String, innerEnum As InnerEnum)
            nameValue = name
            ordinalValue = nextOrdinal
            nextOrdinal += 1
            innerEnumValue = innerEnum
        End Sub

        Public ReadOnly Property Sequential As Boolean
            Get
                Return Equals(SEQUENTIAL_STRATEGY)
            End Get
        End Property

        Public Shared Function values() As CoalitionStrategy()
            Return valueList.ToArray()
        End Function

        Public Function ordinal() As Integer
            Return ordinalValue
        End Function

        Public Overrides Function ToString() As String
            Return nameValue
        End Function

        Public Shared Function valueOf(name As String) As CoalitionStrategy
            For Each enumInstance In valueList
                If enumInstance.nameValue = name Then
                    Return enumInstance
                End If
            Next
            Throw New ArgumentException(name)
        End Function
    End Class

End Namespace


#Region "Microsoft.VisualBasic::bbd7ac8d22596d893e720115e0e6823e, Data_science\Mathematica\Math\Math\Scripting\Helpers\Variable.vb"

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

    '     Class Variable
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [Set], AssignValue, ContainsValue
    ' 
    '         Sub: (+2 Overloads) [Set]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Scripting.Helpers

    Public Class Variable : Inherits MemoryCollection(Of Double)

        ''' <summary>
        ''' Add a variable to the dictionary, if the variable is exists then will update its value.
        ''' (向字典之中添加一个变量，假若该变量存在，则更新他的值)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Default Public Property Variable(Name As String) As Double
            Get
                Dim key As String = Name.ToLower

                If Not objTable.ContainsKey(key) Then
                    Call Warning($"Variable '{Name}' is not exists in the memory, returns ZERO by default!")
                    Return 0
                End If

                Return objTable(key)
            End Get
            Set(value As Double)
                Call [Set](Name, value)
            End Set
        End Property

        Sub New(engine As Expression)
            Call MyBase.New(engine)
            Call MyBase.Add("$", "0", True, False)
        End Sub

        Public Function ContainsValue(x As String) As Boolean
            Return objTable.ContainsKey(x.ToLower)
        End Function

        ''' <summary>
        ''' Add a variable to the dictionary, if the variable is exists then will update its value.
        ''' (向字典之中添加一个变量，假若该变量存在，则更新他的值)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <remarks>
        ''' const [name] [value]
        ''' </remarks>
        Public Function [Set](Name As String, val As String) As Double
            Dim value As Double = __engine.Evaluation(val)
            Call [Set](Name, value)
            Return value
        End Function

        Public Sub [Set](Name As String, val As Double)
            Call MyBase.Add(Name, val, True, False)
        End Sub

        ''' <summary>
        ''' Add a user const from the input of user on the console.
        ''' </summary>
        ''' <param name="statement"></param>
        ''' <remarks></remarks>
        Public Sub [Set](statement As String)
            Dim Name As String = statement.Split.First
            Call [Set](Name, Mid(statement, Len(Name) + 2))
        End Sub

        ''' <summary>
        ''' Assign the new value for a variable
        ''' </summary>
        ''' <param name="statement">var &lt;- new_value_expression</param>
        ''' <remarks></remarks>
        Public Function AssignValue(statement As String) As Double
            Dim Tokens As String() = Strings.Split(statement, "<-")
            Return [Set](Tokens(Scan0), Tokens(1))
        End Function
    End Class
End Namespace

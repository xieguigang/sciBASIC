#Region "Microsoft.VisualBasic::16ecd3475bb1f299d07889ea955be53a, Data_science\Mathematica\Math\ODE\Dynamics\var.vb"

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

    '   Total Lines: 117
    '    Code Lines: 85 (72.65%)
    ' Comment Lines: 7 (5.98%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 25 (21.37%)
    '     File Size: 3.54 KB


    '     Class var
    ' 
    '         Properties: Index, Name, Value
    ' 
    '         Constructor: (+4 Overloads) Sub New
    ' 
    '         Function: Clone, ToString
    ' 
    '         Sub: Assign
    ' 
    '         Operators: <>, =
    ' 
    '     Class NonlinearVar
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clone, Evaluate
    '         Operators: <>, =
    ' 
    '     Interface Ivar
    ' 
    '         Properties: value
    ' 
    '     Interface INonlinearVar
    ' 
    '         Function: Evaluate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language

Namespace Dynamics

    ''' <summary>
    ''' Y variable in the ODE
    ''' </summary>
    Public Class var : Inherits f64
        Implements Ivar
        Implements ICloneable
        Implements IAddress(Of Integer)

        Public Overloads Property Index As Integer Implements IAddress(Of Integer).Address
        Public Overridable Property Name As String Implements IReadOnlyId.Identity
        Public Overrides Property Value As Double Implements Ivar.value

        Public Shared ReadOnly type As Type = GetType(var)

        Sub New()
        End Sub

        Sub New(name$, value#)
            With Me
                .Name = name
                .Value = value
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(name As String)
            Me.Name = name
        End Sub

        ''' <summary>
        ''' Value copy
        ''' </summary>
        ''' <param name="var"></param>
        Sub New(var As var)
            With var
                Index = .Index
                Name = .Name
                Value = .Value
            End With
        End Sub

        Public Overrides Function ToString() As String
            Return $"Dim [{Index}] {Name} As System.Double = {Value}"
        End Function

        Public Overloads Shared Operator =(var As var, x As Double) As var
            var.Value = x
            Return var
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Narrowing Operator CType(x As var) As Integer
            Return x.Index
        End Operator

        Public Overloads Shared Operator <>(var As var, x As Double) As var
            Throw New NotSupportedException
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overridable Function Clone() As Object Implements ICloneable.Clone
            Return New var(Me)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub Assign(address As Integer) Implements IAddress(Of Integer).Assign
            Index = address
        End Sub
    End Class

    Public Class NonlinearVar : Inherits var

        Friend deSolve As Func(Of Double)

        Sub New(solve As Func(Of Double))
            deSolve = solve
        End Sub

        Public Function Evaluate() As Double
            Return deSolve()
        End Function

        Public Overrides Function Clone() As Object
            Return New NonlinearVar(deSolve) With {
                .Index = Index,
                .Name = Name,
                .Value = Value
            }
        End Function

        Public Overloads Shared Operator =(var As NonlinearVar, equation As Func(Of Double)) As var
            var.deSolve = equation
            Return var
        End Operator

        Public Overloads Shared Operator <>(var As NonlinearVar, equation As Func(Of Double)) As var
            Throw New NotImplementedException
        End Operator
    End Class

    Public Interface Ivar : Inherits IReadOnlyId

        Property value As Double
    End Interface

    Public Interface INonlinearVar : Inherits Ivar

        Function Evaluate() As Double
    End Interface
End Namespace

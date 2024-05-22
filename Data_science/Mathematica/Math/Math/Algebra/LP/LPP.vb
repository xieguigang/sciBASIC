#Region "Microsoft.VisualBasic::b1ff0f0229bbb02f4c5bb2006949e174, Data_science\Mathematica\Math\Math\Algebra\LP\LPP.vb"

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

    '   Total Lines: 203
    '    Code Lines: 132 (65.02%)
    ' Comment Lines: 45 (22.17%)
    '    - Xml Docs: 88.89%
    ' 
    '   Blank Lines: 26 (12.81%)
    '     File Size: 9.78 KB


    '     Class LPP
    ' 
    '         Properties: DecimalFormat, ObjectFunctionVariables, PIVOT_ITERATION_LIMIT, USE_SUBSCRIPT_UNICODE
    ' 
    '         Constructor: (+3 Overloads) Sub New
    ' 
    '         Function: ArtificialVariableAssignments, increaseArtificialVariableIndices, solve, ToString
    ' 
    '         Sub: addArtificialVariables, addVariableAt, (+2 Overloads) makeStandardForm
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace LinearAlgebra.LinearProgramming

    ''' <summary>
    ''' solve continuous-space linear programming problems by the simplex method.
    ''' </summary>
    ''' <remarks>
    ''' java source: https://github.com/gthole/lpp
    ''' </remarks>
    Public Class LPP

        Friend ReadOnly objectiveFunctionType As OptimizationType
        ''' <summary>
        ''' 这个变量名称列表之中会添加拓展的新的变量名称
        ''' 
        ''' 可以使用objectfunction的系数长度来取出原来的输入的变量名称的列表
        ''' </summary>
        Friend ReadOnly variableNames As List(Of String)
        Friend ReadOnly objectiveFunctionCoefficients As List(Of Double)
        Friend ReadOnly constraintCoefficients() As List(Of Double)
        Friend ReadOnly constraintTypes() As String
        Friend ReadOnly constraintRightHandSides() As Double

        Friend objectiveFunctionValue As Double

        Public Shared Property PIVOT_ITERATION_LIMIT As Integer = 1000
        Public Shared Property USE_SUBSCRIPT_UNICODE As Boolean = False
        Public Shared Property DecimalFormat As String = "G5"

        Public ReadOnly Property ObjectFunctionVariables As String()
            Get
                Return variableNames.Take(objectiveFunctionCoefficients.Count).ToArray
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="opt">目标函数的类型，是求取极大值还是极小值</param>
        ''' <param name="variableNames">方程之中的未知变量的名称，可以省略这个函数，程序会默认会自动使用x1, x2, x3...等来自动命名</param>
        ''' <param name="objectiveFunctionCoefficients">目标函数之中每一个未知变量所对应的系数</param>
        ''' <param name="constraintCoefficients">方程组的左边：系数矩阵</param>
        ''' <param name="constraintTypes">方程组之中的函数类型：大于，小于，等于</param>
        ''' <param name="constraintRightHandSides">方程组的右边：方程组之中每一个方程的结果值</param>
        ''' <param name="objectiveFunctionValue">目标方程的目标结果值</param>
        Sub New(opt As OptimizationType,
                variableNames$(),
                objectiveFunctionCoefficients#(),
                constraintCoefficients#(,),
                constraintTypes$(),
                constraintRightHandSides#(),
                Optional objectiveFunctionValue# = 0)

            Call Me.New(opt.Description, variableNames, objectiveFunctionCoefficients, constraintCoefficients.ToVectorList, constraintTypes, constraintRightHandSides, objectiveFunctionValue)
        End Sub

        Sub New(lppModel As LPPModel)
            Call Me.New(
                objectiveFunctionType:=lppModel.objectiveFunctionType,
                variableNames:=lppModel.variableNames,
                objectiveFunctionCoefficients:=lppModel.objectiveFunctionCoefficients,
                constraintCoefficients:=lppModel.ParseMatrix,
                constraintTypes:=lppModel.constraintTypes,
                constraintRightHandSides:=lppModel.constraintRightHandSides,
                objectiveFunctionValue:=lppModel.objectiveFunctionValue
            )
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="objectiveFunctionType$">目标函数的类型，是求取极大值还是极小值</param>
        ''' <param name="variableNames">方程之中的未知变量的名称，可以省略这个函数，程序会默认会自动使用x1, x2, x3...等来自动命名</param>
        ''' <param name="objectiveFunctionCoefficients">目标函数之中每一个未知变量所对应的系数</param>
        ''' <param name="constraintCoefficients">方程组的左边：系数矩阵</param>
        ''' <param name="constraintTypes">方程组之中的函数类型：大于，小于，等于</param>
        ''' <param name="constraintRightHandSides">方程组的右边：方程组之中每一个方程的结果值</param>
        ''' <param name="objectiveFunctionValue">目标方程的目标结果值</param>
        Public Sub New(objectiveFunctionType$,
                       variableNames() As String,
                       objectiveFunctionCoefficients() As Double,
                       constraintCoefficients()() As Double,
                       constraintTypes() As String,
                       constraintRightHandSides() As Double,
                       objectiveFunctionValue As Double)

            ' Create default variable name array
            If variableNames Is Nothing OrElse variableNames.Length = 0 Then
                variableNames = New String(objectiveFunctionCoefficients.Length - 1) {}

                For i As Integer = 0 To variableNames.Length - 1
                    variableNames(i) = "x" & subscriptN(i)
                Next
            End If

            ' Validation
            If constraintTypes.Length <> constraintRightHandSides.Length OrElse constraintRightHandSides.Length <> constraintCoefficients.Length Then
                Throw New Exception("LPP constraints do not appear well-formed.")
            End If
            If variableNames.Length <> objectiveFunctionCoefficients.Length Then
                Throw New Exception("LPP objective function does not appear well-formed.")
            End If
            For i As Integer = 0 To constraintCoefficients.Length - 1
                If constraintCoefficients(i).Length <> variableNames.Length Then
                    Throw New Exception($"LPP constraint {i} is not of the same size length as the objective function.")
                End If
            Next

            Me.objectiveFunctionType = objectiveFunctionType.ParseType
            Me.variableNames = variableNames.ToList
            Me.objectiveFunctionCoefficients = objectiveFunctionCoefficients.ToList
            Me.constraintCoefficients = constraintCoefficients _
                .Select(Function(v) v.ToList) _
                .ToArray
            Me.constraintTypes = constraintTypes
            Me.constraintRightHandSides = constraintRightHandSides
            Me.objectiveFunctionValue = objectiveFunctionValue
        End Sub

        Public Overrides Function ToString() As String
            Return LPPDebugView.ToString(Me)
        End Function

        ''' <summary>
        ''' Change Signs to = by adding variables
        ''' </summary>
        Friend Sub makeStandardForm()
            For i As Integer = 0 To constraintTypes.Length - 1
                If constraintTypes(i) <> "=" Then
                    addVariableAt(i, If(constraintTypes(i) = "≥" OrElse constraintTypes(i) = ">=", -1, 1))
                    constraintTypes(i) = "="
                End If
            Next
        End Sub

        ''' <summary>
        ''' Change signs to = by adding variables
        ''' </summary>
        ''' <param name="artificialVariables"></param>
        Friend Sub makeStandardForm(artificialVariables As List(Of Integer))
            For i As Integer = 0 To constraintTypes.Length - 1
                If constraintTypes(i) <> "=" Then
                    addVariableAt(i, If(constraintTypes(i) = "≥" OrElse constraintTypes(i) = ">=", -1, 1))
                    constraintTypes(i) = "="
                    artificialVariables = increaseArtificialVariableIndices(artificialVariables)
                End If
            Next
        End Sub

        Private Shared Function increaseArtificialVariableIndices(artificialVariables As List(Of Integer)) As List(Of Integer)
            For Each artificialVariable As Integer In artificialVariables
                If artificialVariable <> -1 Then
                    artificialVariable += 1
                End If
            Next

            Return artificialVariables
        End Function

        Friend Function ArtificialVariableAssignments() As List(Of Integer)
            Dim assignments As New List(Of Integer)()
            Dim k As Integer = 0

            For j As Integer = 0 To constraintTypes.Length - 1
                If constraintTypes(j) = "=" Then
                    assignments.Add(objectiveFunctionCoefficients.Count + k)
                    k += 1
                Else
                    assignments.Add(-1)
                End If
            Next

            Return assignments
        End Function

        Friend Sub addArtificialVariables(artificialVariables As List(Of Integer))
            For j As Integer = 0 To constraintTypes.Length - 1
                If artificialVariables(j) <> -1 Then
                    Me.addVariableAt(j, 1)
                End If
            Next
        End Sub

        ''' <summary>
        ''' Unfortunate copy and pasting going on here.
        ''' </summary>
        ''' <param name="constraintIndex"></param>
        ''' <param name="value"></param>
        Private Sub addVariableAt(constraintIndex As Integer, value As Double)
            variableNames.Add("v" & subscriptN(variableNames.Count + 1))
            objectiveFunctionCoefficients.Add(0)

            For j As Integer = 0 To constraintCoefficients.Length - 1
                constraintCoefficients(j).Add(If(j <> constraintIndex, 0, value))
            Next
        End Sub

        Public Function solve(Optional showProgress As Boolean = True) As LPPSolution
            Return New LPPSolver(Me).Solve(showProgress)
        End Function
    End Class
End Namespace

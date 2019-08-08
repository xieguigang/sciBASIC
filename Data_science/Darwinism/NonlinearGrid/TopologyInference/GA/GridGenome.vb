Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Public Interface IGridFitness : Inherits IDynamicsComponent(Of IGridFitness)

    Function CalculateError(status As Vector, target As Double) As Double
End Interface

Public MustInherit Class GridGenome(Of T As IDynamicsComponent(Of T))
    Implements IGridFitness

    Public ReadOnly Property chromosome As T

    ''' <summary>
    ''' Number of system variables.
    ''' </summary>
    Protected ReadOnly width As Integer
    ''' <summary>
    ''' 约束变异所产生的值的上限
    ''' </summary>
    Protected ReadOnly truncate As Double
    Protected ReadOnly rangePositive As Boolean

    ''' <summary>
    ''' 突变程度
    ''' </summary>
    Public Overridable Property MutationRate As Double

    Protected Overridable ReadOnly Property IDynamicsComponent_Width As Integer Implements IDynamicsComponent(Of IGridFitness).Width
        Get
            Return width
        End Get
    End Property

    Public Const CrossOverRate As Double = 30

    Sub New(chr As T, mutationRate As Double, truncate As Double, rangePositive As Boolean)
        Me.chromosome = chr
        Me.width = chr.Width
        Me.MutationRate = mutationRate
        Me.truncate = truncate
        Me.rangePositive = rangePositive
    End Sub

    ''' <summary>
    ''' <see cref="GridSystem.Evaluate(Vector)"/>
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overridable Function Evaluate(X As Vector) As Double Implements IGridFitness.Evaluate
        Return chromosome.Evaluate(X)
    End Function

    Public Function CalculateError(status As Vector, target As Double) As Double Implements IGridFitness.CalculateError
        Dim predicts = Evaluate(status)

        If rangePositive AndAlso predicts < 0 Then
            Return target
        ElseIf predicts.IsNaNImaginary Then
            Return Double.MaxValue
        Else
            Return Math.Abs(predicts - target)
        End If
    End Function

    Protected Function valueMutate(x As Double) As Double
        If x = 0R Then
            Return 1
        ElseIf FlipCoin() Then
            x += randf.randf(0, x * MutationRate)
        Else
            x -= randf.randf(0, x * MutationRate)
        End If

        If Math.Abs(x) > truncate Then
            x = Math.Sign(x) * randf.seeds.NextDouble * truncate
        End If

        Return x
    End Function

    Public MustOverride Function Clone() As IGridFitness Implements ICloneable(Of IGridFitness).Clone
End Class

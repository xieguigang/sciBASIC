#Region "Microsoft.VisualBasic::bb2cc772f6e0763e5c4370f869759569, Data_science\Mathematica\Math\ODE\ODEsSolver\GenericODEs.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Delegate Sub
    ' 
    ' 
    ' Class GenericODEs
    ' 
    '     Properties: df
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: y0
    ' 
    '     Sub: func
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Delegate Sub [Function](dx As Double, ByRef dy As Vector)

Public Class GenericODEs : Inherits ODEs

    ''' <summary>
    ''' df(dx As <see cref="Double"/>, ByRef dy As <see cref="Vector"/>)
    ''' </summary>
    ''' <returns></returns>
    Public Property df As [Function]

    Sub New(ParamArray vars As var())
        Me.vars = vars

        For Each x As SeqValue(Of var) In vars.SeqIterator
            x.value.Index = x.i
        Next
    End Sub

    Sub New(vars As var(), df As [Function])
        Call Me.New(vars)
        Me.df = df
    End Sub

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        Call _df(dx, dy)
    End Sub

    Protected Overrides Function y0() As var()
        Return vars
    End Function
End Class

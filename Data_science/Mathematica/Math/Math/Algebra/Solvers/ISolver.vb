#Region "Microsoft.VisualBasic::b69dd2ee9a686a0a2bc79fa8cdb8e3b6, Data_science\Mathematica\Math\Math\Algebra\Solvers\ISolver.vb"

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

    '     Delegate Function
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace LinearAlgebra.Solvers

    ''' <summary>
    ''' ``a*b=0 -> x``
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns>x</returns>
    ''' <remarks></remarks>
    Public Delegate Function Solve(A As Matrix, b As Vector) As Vector

End Namespace

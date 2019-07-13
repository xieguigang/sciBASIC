#Region "Microsoft.VisualBasic::8ca0a1a909608a48eaba1a132a6fd647, Data_science\Mathematica\Math\Math\Algebra\RSyntax\PrimitiveAPI\Etc.vb"

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

    '     Module [Is]
    ' 
    '         Function: Finite, NAN
    ' 
    '         Module [Double]
    ' 
    '             Properties: Eps
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace SyntaxAPI

    <Package("RBase.Is")>
    Public Module [Is]

        ''' <summary>
        ''' is.finite and is.infinite return a vector of the same length as x, indicating which elements are finite (not infinite and not missing) or infinite.
        ''' </summary>
        ''' <param name="x">R object to be tested: the default methods handle atomic vectors.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("is.finite")>
        Public Function Finite(x As Vector) As BooleanVector
            Return New BooleanVector(From n In x Select Not Double.IsInfinity(n))
        End Function

        <ExportAPI("is.NAN")>
        Public Function NAN(x As Vector) As BooleanVector
            Return New BooleanVector(From n In x Select Double.IsNaN(n))
        End Function

    End Module

    Namespace Machine

        <Package("RBase.Machine.Double")>
        Public Module [Double]

            Public ReadOnly Property Eps As Double
                Get
                    Return Double.Epsilon
                End Get
            End Property
        End Module
    End Namespace
End Namespace

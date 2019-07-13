#Region "Microsoft.VisualBasic::07e08422335d228a158f898d1d9752a0, Data_science\Mathematica\Math\Math\Algebra\Vector\VectorEqualityComparer.vb"

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

    '     Class VectorEqualityComparer
    ' 
    '         Function: Equals, GetHashCode, (+2 Overloads) VectorEqualsToAnother
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace LinearAlgebra

    ''' <summary>
    ''' Determine that the two vector are equals to each other.
    ''' </summary>
    ''' <remarks>单实例模式？？</remarks>
    Public Class VectorEqualityComparer : Implements IEqualityComparer(Of Vector)

        ''' <summary>
        ''' Sequence Equals
        ''' </summary>
        ''' <param name="list1"></param>
        ''' <param name="list2"></param>
        ''' <returns></returns>
        Public Shared Function VectorEqualsToAnother(list1 As List(Of Double), list2 As List(Of Double)) As Boolean
            If list1.Count <> list2.Count Then
                Return False
            End If

            For i As Integer = 0 To list1.Count - 1
                If list1(i) <> list2(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        Public Shared Function VectorEqualsToAnother(v1#(), v2#()) As Boolean
            If v1.Length <> v2.Length Then
                Return False
            End If

            For i As Integer = 0 To v1.Length - 1
                If v1(i) <> v2(i) Then
                    Return False
                End If
            Next

            Return True
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Function Equals(x As Vector, y As Vector) As Boolean Implements IEqualityComparer(Of Vector).Equals
            Return VectorEqualsToAnother(list1:=x, list2:=y)
        End Function

        ''' <summary>
        ''' Sum all elements' hashcode 
        ''' </summary>
        ''' <param name="v"></param>
        ''' <returns></returns>
        Public Overloads Function GetHashCode(v As Vector) As Integer Implements IEqualityComparer(Of Vector).GetHashCode
            Dim sum As Integer = v.Sum(Function(x) x.GetHashCode)
            Return sum
        End Function
    End Class
End Namespace

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Scripting

    Public Class VectorModel(Of T) : Inherits VectorShadows(Of T)

        Default Public Shadows ReadOnly Property Vector(name$) As Vector
            Get
                Dim v As Object = Nothing

                If Not MyBase.TryGetMember(name, result:=v) Then
                    Throw New EntryPointNotFoundException(name)
                Else
                    Select Case v.GetType
                        Case GetType(VectorShadows(Of Double))
                            Return DirectCast(v, VectorShadows(Of Double)).AsVector
                        Case GetType(VectorShadows(Of Single))
                            Return DirectCast(v, VectorShadows(Of Single)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Integer))
                            Return DirectCast(v, VectorShadows(Of Integer)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Long))
                            Return DirectCast(v, VectorShadows(Of Long)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Boolean))
                            Return DirectCast(v, VectorShadows(Of Boolean)).Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of Char))
                            Return DirectCast(v, VectorShadows(Of Char)).CharCodes.Select(Function(x) CDbl(x)).AsVector
                        Case GetType(VectorShadows(Of String))
                            Return DirectCast(v, VectorShadows(Of String)).Select(Function(s) s.ParseDouble).AsVector

                        Case Else

                            Throw New NotSupportedException(v.GetType.FullName)

                    End Select
                End If
            End Get
        End Property

        Sub New(source As IEnumerable(Of T))
            Call MyBase.New(source)
        End Sub
    End Class
End Namespace
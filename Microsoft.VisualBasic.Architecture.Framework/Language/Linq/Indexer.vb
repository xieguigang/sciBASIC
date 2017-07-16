Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates

Namespace Language

    Public Module Indexer

#Region "Default Public Overloads Property Item(args As Object) As T()"
        Public Function Indexing(args As Object) As IEnumerable(Of Integer)
            Dim type As Type = args.GetType

            If type Is GetType(Integer) Then
                Return {DirectCast(args, Integer)}
            ElseIf type.ImplementsInterface(GetType(IEnumerable(Of Integer))) Then
                Return DirectCast(args, IEnumerable(Of Integer))
            ElseIf type.ImplementsInterface(GetType(IEnumerable(Of Boolean))) Then
                Return Which.IsTrue(DirectCast(args, IEnumerable(Of Boolean)))
            ElseIf type.ImplementsInterface(GetType(IEnumerable(Of Object))) Then
                Dim array = DirectCast(args, IEnumerable(Of Object)).ToArray

                With array(Scan0).GetType
                    If .ref Is GetType(Boolean) Then
                        Return Which.IsTrue(array.Select(Function(o) CBool(o)))
                    ElseIf .ref Is GetType(Integer) Then
                        Return array.Select(Function(o) CInt(o))
                    Else
                        Throw New NotImplementedException
                    End If
                End With
            Else
                Throw New NotImplementedException
            End If
        End Function
#End Region
    End Module
End Namespace
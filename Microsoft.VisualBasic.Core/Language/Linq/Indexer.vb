#Region "Microsoft.VisualBasic::a4340f380698df28410faf9ff7a6a08f, Microsoft.VisualBasic.Core\Language\Linq\Indexer.vb"

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

    '     Module Indexer
    ' 
    '         Function: Indexing
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq

Namespace Language

    Public Module Indexer

#Region "Default Public Overloads Property Item(args As Object) As T()"

        ''' <summary>
        ''' Generates the vector elements index collection.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function Indexing(args As Object) As IEnumerable(Of Integer)
            Dim type As Type = args.GetType

            If type Is GetType(Integer) Then
                Return {DirectCast(args, Integer)}
            ElseIf type.ImplementInterface(GetType(IEnumerable(Of Integer))) Then
                Return DirectCast(args, IEnumerable(Of Integer))
            ElseIf type.ImplementInterface(GetType(IEnumerable(Of Boolean))) Then
                Return Which.IsTrue(DirectCast(args, IEnumerable(Of Boolean)))
            ElseIf type.ImplementInterface(GetType(IEnumerable(Of Object))) Then
                Dim array = DirectCast(args, IEnumerable(Of Object)).ToArray

                With array(Scan0).GetType
                    If .ByRef Is GetType(Boolean) Then
                        Return Which.IsTrue(array.Select(Function(o) CBool(o)))
                    ElseIf .ByRef Is GetType(Integer) Then
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

#Region "Microsoft.VisualBasic::a379a2f5bc31ccee1e43102f7af78524, Microsoft.VisualBasic.Core\Language\Value\Clones.vb"

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

    '     Module Clones
    ' 
    '         Function: (+5 Overloads) Clone, CloneCopy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Language.Values

    ''' <summary>
    ''' Some extension for copy a collection object.
    ''' </summary>
    Public Module Clones

        ''' <summary>
        ''' Creates a new dictionary
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="V"></typeparam>
        ''' <param name="table"></param>
        ''' <returns></returns>
        <Extension> Public Function Clone(Of T, V)(table As IDictionary(Of T, V)) As Dictionary(Of T, V)
            Return New Dictionary(Of T, V)(table)
        End Function

        <Extension> Public Function Clone(Of T)(list As List(Of T)) As List(Of T)
            Return New List(Of T)(list)
        End Function

        <Extension> Public Function CloneCopy(Of T)(array As T()) As T()
            Return DirectCast(array.Clone, T())
        End Function

        <Extension> Public Function Clone(s As String) As String
            Return New String(s.ToCharArray)
        End Function

        <Extension> Public Function Clone(int As VBInteger) As VBInteger
            Return New VBInteger(int.value)
        End Function

        <Extension> Public Function Clone(float As VBDouble) As VBDouble
            Return New VBDouble(float.value)
        End Function
    End Module
End Namespace

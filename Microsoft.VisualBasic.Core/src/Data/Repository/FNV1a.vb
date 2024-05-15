#Region "Microsoft.VisualBasic::f143c3ac5e162b690da25f9ddd694bd0, Microsoft.VisualBasic.Core\src\Data\Repository\FNV1a.vb"

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

    '   Total Lines: 114
    '    Code Lines: 72
    ' Comment Lines: 25
    '   Blank Lines: 17
    '     File Size: 4.23 KB


    '     Module FNV1a
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetDeterministicHashCode, (+5 Overloads) GetHashCode, getHashValue
    ' 
    '         Sub: RegisterHashFunction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Data.Repository

    Public Module FNV1a

        ReadOnly hashHandler As New Dictionary(Of Type, Func(Of Object, Integer))

        Sub New()
            hashHandler(GetType(String)) = AddressOf GetDeterministicHashCode
        End Sub

        ''' <summary>
        ''' What if you need GetHashCode() to be deterministic across program executions?
        ''' 
        ''' https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        ''' </summary>
        ''' <param name="str"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetDeterministicHashCode(str As String) As UInteger
            Dim hash1 As UInteger = (5381 << 16) + 5381
            Dim hash2 As UInteger = hash1

            For i As Integer = 0 To str.Length - 1 Step 2
                hash1 = (hash1 << 5) + hash1 Xor AscW(str(i))

                If i = str.Length - 1 Then
                    Exit For
                Else
                    hash2 = (hash2 << 5) + hash2 Xor AscW(str(i + 1))
                End If
            Next

            Return hash1 + hash2 * 1566083941
        End Function

        <Extension>
        Public Function GetHashCode(Of T)(x As T, getFields As Func(Of T, IEnumerable(Of Object))) As Integer
            Return GetHashCode(getFields(x))
        End Function

        Public Function GetHashCode(getValues As Func(Of IEnumerable(Of Object))) As Integer
            Return GetHashCode(getValues())
        End Function

        Public Sub RegisterHashFunction(target As Type, hash As Func(Of Object, Integer))
            hashHandler(target) = hash
        End Sub

        Private Function getHashValue(obj As Object) As Integer
            If TypeOf obj Is Integer Then
                Return DirectCast(obj, Integer)
            ElseIf hashHandler.ContainsKey(obj.GetType) Then
                Return hashHandler(obj.GetType)(obj)
            Else
                Return obj.GetHashCode
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetHashCode(str As String) As UInteger
            Return FNV1a.GetHashCode({str})
        End Function

        Public Function GetHashCode(targets As IEnumerable(Of String)) As UInteger
            Const offset As UInteger = 2166136261
            Const prime As Long = 16777619

            Return targets.Aggregate(
                seed:=offset,
                func:=Function(hashCode As UInteger, value As String) As UInteger
                          If value Is Nothing OrElse value = "" Then
                              Return (hashCode Xor 0) * prime
                          Else
                              Return (hashCode Xor CLng(GetDeterministicHashCode(value))) * prime
                          End If
                      End Function)
        End Function

        ''' <summary>
        ''' get FNV-1a hascode
        ''' 
        ''' ```
        ''' hash = offset_basis
        ''' 
        ''' for each octet_of_data to be hashed
        '''   hash = hash Xor octet_of_data
        '''   hash = hash * FNV_prime
        '''   
        ''' return hash
        ''' ```
        ''' </summary>
        ''' <param name="targets"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://www.isthe.com/chongo/tech/comp/fnv/index.html
        ''' </remarks>
        Public Function GetHashCode(targets As IEnumerable(Of Object)) As UInteger
            Const offset As UInteger = 2166136261
            Const prime As Long = 16777619

            Return targets.Aggregate(
                seed:=offset,
                func:=Function(hashCode As UInteger, value As Object)
                          If value Is Nothing Then
                              Return (hashCode Xor 0) * prime
                          Else
                              Return (hashCode Xor getHashValue(value)) * prime
                          End If
                      End Function)
        End Function
    End Module
End Namespace

#Region "Microsoft.VisualBasic::3bb53e581cd51e2fe4257034fd3c40cf, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/DataFrame//DataFrame/NumericDataSet.vb"

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

    '   Total Lines: 70
    '    Code Lines: 60
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 2.64 KB


    ' Module NumericDataSet
    ' 
    '     Function: IndexGetter, NumericGetter, NumericMatrix
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Public Module NumericDataSet

    <Extension>
    Private Function IndexGetter(v As Double()) As Func(Of Integer, Double)
        If v.IsNullOrEmpty Then
            Return Function(any) 0.0
        ElseIf v.Length = 1 Then
            Dim scalar As Double = v(0)
            Return Function(any) scalar
        Else
            Return Function(i) v(i)
        End If
    End Function

    <Extension>
    Public Function NumericGetter(v As FeatureVector) As Func(Of Integer, Double)
        Select Case v.type
            Case GetType(Double), GetType(Single), GetType(Integer),
                 GetType(Long), GetType(UInteger), GetType(ULong),
                 GetType(Short), GetType(UShort),
                 GetType(Boolean),
                 GetType(DateTime),
                 GetType(Byte), GetType(SByte)

                Return v.TryCast(Of Double)().IndexGetter
            Case GetType(String), GetType(Char)
                Dim factors As Index(Of String) = v.vector _
                    .AsObjectEnumerator _
                    .Select(Function(s) any.ToString(s)) _
                    .Indexing
                Dim vals As Double() = DirectCast(v.vector, String()) _
                    .Select(Function(f) CDbl(factors(f))) _
                    .ToArray

                Call $"cast the feature {v.name} in character type to numeric vector.".Warning

                Return vals.IndexGetter
            Case Else
                Throw New NotImplementedException($"could not cast object of type '{v.type.Name}' to numeric value!")
        End Select
    End Function

    <Extension>
    Public Iterator Function NumericMatrix(df As DataFrame) As IEnumerable(Of NamedCollection(Of Double))
        Dim colnames As String() = df.featureNames
        Dim fieldGetters As Func(Of Integer, Double)() = colnames _
            .Select(Function(s) df(s).NumericGetter) _
            .ToArray
        Dim nrow As Integer = df.nsamples
        Dim rownames As String() = df.rownames
        Dim offset As Integer
        Dim row As Double()

        For i As Integer = 0 To nrow - 1
            offset = i
            row = fieldGetters _
                .Select(Function(v) v(offset)) _
                .ToArray

            Yield New NamedCollection(Of Double)(rownames(i), row)
        Next
    End Function

End Module


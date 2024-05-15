#Region "Microsoft.VisualBasic::3bb77958f11d187e82360ee4631b5d1d, Data_science\Mathematica\Math\Math.Statistics\Distributions\ContinuousDistribution.vb"

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

    '   Total Lines: 202
    '    Code Lines: 125
    ' Comment Lines: 48
    '   Blank Lines: 29
    '     File Size: 9.76 KB


    '     Class ContinuousDistribution
    ' 
    '         Properties: PeriodOfRecord
    ' 
    '         Function: AndersonDarlingTest, (+2 Overloads) BootStrap, Clone, Equals, GetCDF
    '                   GetHashCode, GetInvCDF, GetParamNames, GetParamValues, GetPDF
    '                   Kolmogorov_SmirnovTest
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Math.LinearAlgebra

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions
    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public MustInherit Class ContinuousDistribution

        Public Property PeriodOfRecord As Integer

        ''' <summary>
        ''' This function produces a value for a given probability, this value will represent the Non-Exceedance value for that probability. </summary>
        ''' <param name="probability"> a number between 0 and 1. </param>
        ''' <returns> a value distributed by the distribution defined in the concrete implementation of this abstract class. </returns>
        Public MustOverride Function GetInvCDF(probability As Double) As Double
        ''' <summary>
        ''' This function is the Cumulative Distribution Function. It returns a Non Exceedance probability for any value.  It will be implemented by all concrete implementations of this abstract class. </summary>
        ''' <param name="value"> the value that a probability will be produced for. </param>
        ''' <returns> a probability that this value will be exceeded by any other value from the sample set. </returns>
        Public MustOverride Function GetCDF(value As Double) As Double
        ''' <summary>
        ''' This is the Probability Density Function. It describes the likelihood any given value will occur within a dataset. </summary>
        ''' <param name="value"> the value that a likelihood will be returned for. </param>
        ''' <returns> the likelihood (defined by the concrete distribution) the specified value will occur in any given sample dataset (assuming the value is from the underlying distribution). </returns>
        Public MustOverride Function GetPDF(value As Double) As Double

        ''' <summary>
        ''' This function produces a value for a given probability, this value will represent the Non-Exceedance value for that probability. </summary>
        ''' <param name="probability"> a number between 0 and 1. </param>
        ''' <returns> a value distributed by the distribution defined in the concrete implementation of this abstract class. </returns>
        Public Function GetInvCDF(probability As Vector) As Vector
            Dim v As New List(Of Double)

            For Each x As Double In probability
                Call v.Add(GetInvCDF(x))
            Next

            Return New Vector(v)
        End Function

        ''' <summary>
        ''' This function is the Cumulative Distribution Function. It returns a Non Exceedance probability for any value.  It will be implemented by all concrete implementations of this abstract class. </summary>
        ''' <param name="value"> the value that a probability will be produced for. </param>
        ''' <returns> a probability that this value will be exceeded by any other value from the sample set. </returns>
        Public Function GetCDF(value As Vector) As Vector
            Dim v As New List(Of Double)

            For Each x As Double In value
                Call v.Add(GetCDF(x))
            Next

            Return New Vector(v)
        End Function

        ''' <summary>
        ''' This is the Probability Density Function. It describes the likelihood any given value will occur within a dataset. </summary>
        ''' <param name="value"> the value that a likelihood will be returned for. </param>
        ''' <returns> the likelihood (defined by the concrete distribution) the specified value will occur in any given sample dataset (assuming the value is from the underlying distribution). </returns>
        Public Function GetPDF(value As Vector) As Vector
            Dim v As New List(Of Double)

            For Each x As Double In value
                Call v.Add(GetPDF(x))
            Next

            Return New Vector(v)
        End Function

        Public MustOverride Function Validate() As IEnumerable(Of Exception)

        ' <editor-fold defaultstate="collapsed" desc="Goodness of fit tests">
        Public Overridable Function Kolmogorov_SmirnovTest() As Double
            ' need to create a good empirical distribution.
            Return 0
        End Function

        Public Overridable Function AndersonDarlingTest() As Double
            'still need a good emperical distribution.
            Return 0
        End Function
        ' </editor-fold>
        ' <editor-fold defaultstate="collapsed" desc="Reflection based utilities, GetParamNames, GetParamValues, Clone, equals, hash, ReadFromXML, WriteToXML">

        ''' <summary>
        ''' This function will return string representations of the parameter names for each distribution. </summary>
        ''' <returns> a String array of all of the declared fields composing the concrete implementation of this ContinuousDistribution </returns>
        Public Overridable Function GetParamNames() As String()
            Dim flds As FieldInfo() = CType(Me.GetType(), TypeInfo).DeclaredFields
            Dim ParamNames As String() = New String(flds.Length - 1) {}
            For i As Integer = 0 To flds.Length - 1
                ParamNames(i) = flds(i).Name
            Next
            Return ParamNames
        End Function

        ''' <summary>
        ''' This function determines the current values for each parameter in this concrete implementation of the ContinuousDistribution </summary>
        ''' <returns> an array of object for each parameter in this class. </returns>
        Public Overridable Function GetParamValues() As Object()
            Dim flds As FieldInfo() = CType(Me.GetType(), TypeInfo).DeclaredFields
            Dim ParamVals As Object() = New Object(flds.Length - 1) {}
            For i As Integer = 0 To flds.Length - 1
                Try
                    Select Case flds(i).FieldType.Name
                        Case "double"
                            ParamVals(i) = flds(i).GetDouble(Me)
                        Case "int"
                            ParamVals(i) = flds(i).GetInt(Me)

                        Case Else
                    End Select

                Catch ex As Exception 'System.ArgumentException Or IllegalAccessException ex

                End Try
            Next
            Return ParamVals
        End Function

        ''' <summary>
        ''' Creates a clone of the current ContinuousDistribution. </summary>
        ''' <returns> A ContinuousDistribution of the same type as the one this function is called on. </returns>
        Public Overridable Function Clone() As ContinuousDistribution
            'create a new continuousdistribution and populate it from this using reflection.
            Dim Dist As ContinuousDistribution = Nothing
            Dim c As Type

            Try
                c = Type.GetType(Me.GetType().Name)
                Dist = CType(c.GetConstructor().NewInstance(), ContinuousDistribution)
                Dim flds As FieldInfo() = CType(c, TypeInfo).DeclaredFields
                For Each f As FieldInfo In flds
                    Select Case f.FieldType.Name
                        Case "double"
                            f.SetValue(Dist, f.GetDouble(Me))
                        Case "int"
                            f.SetValue(Dist, f.GetInt(Me))
                        Case Else
                    End Select
                Next
            Catch ex As Exception

            End Try

            Return Dist
        End Function

        Public Overrides Function Equals(dist As Object) As Boolean
            If dist.GetType().Name.Equals(Me.GetType().Name) Then
                Dim thisParamValues As Object() = Me.GetParamValues()
                Dim those As ContinuousDistribution = CType(dist, ContinuousDistribution)
                Dim thoseParamValues As Object() = those.GetParamValues()
                If thisParamValues.Length = thoseParamValues.Length Then
                    For i As Integer = 0 To thisParamValues.Length - 1
                        If thisParamValues(i) IsNot thoseParamValues(i) Then Return False
                    Next
                Else
                    Return False
                End If
            Else
                Return False
            End If
            Return True
        End Function

        Public Overrides Function GetHashCode() As Integer
            Dim hash As Integer = Me.GetType().Name.GetHashCode()
            Dim vals As Object() = Me.GetParamValues()
            For Each val As Object In vals
                hash += val.GetHashCode()
            Next val
            Return hash
        End Function

        Public Overridable Function BootStrap() As Double()
            Dim result As Double() = New Double(_PeriodOfRecord - 1) {}
            Dim Random As New Random
            For i As Integer = 0 To _PeriodOfRecord - 1
                result(i) = GetInvCDF(Random.NextDouble())
            Next
            Return result
        End Function

        Public Overridable Function BootStrap(seed As Long) As Double()
            Dim result As Double() = New Double(_PeriodOfRecord - 1) {}
            Dim Random As New Random(seed)
            For i As Integer = 0 To _PeriodOfRecord - 1
                result(i) = GetInvCDF(Random.NextDouble())
            Next
            Return result
        End Function
    End Class

End Namespace

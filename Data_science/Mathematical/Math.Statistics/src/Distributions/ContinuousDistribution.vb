#Region "Microsoft.VisualBasic::248ae215d5466516f342d26d3f26bf20, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\src\Distributions\ContinuousDistribution.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Reflection
Imports Microsoft.VisualBasic.Language.Java

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
        Private _PeriodOfRecord As Integer = 0
        ''' <summary>
        ''' This function produces a value for a given probability, this value will represent the Non-Exceedance value for that probability. </summary>
        ''' <param name="probability"> a number between 0 and 1. </param>
        ''' <returns> a value distributed by the distribution defined in the concrete implementation of this abstract class. </returns>
        Public MustOverride Function GetInvCDF( probability As Double) As Double
        ''' <summary>
        ''' This function is the Cumulative Distribution Function. It returns a Non Exceedance probability for any value.  It will be implemented by all concrete implementations of this abstract class. </summary>
        ''' <param name="value"> the value that a probability will be produced for. </param>
        ''' <returns> a probability that this value will be exceeded by any other value from the sample set. </returns>
        Public MustOverride Function GetCDF( value As Double) As Double
        ''' <summary>
        ''' This is the Probability Density Function. It describes the likelihood any given value will occur within a dataset. </summary>
        ''' <param name="value"> the value that a likelihood will be returned for. </param>
        ''' <returns> the likelihood (defined by the concrete distribution) the specified value will occur in any given sample dataset (assuming the value is from the underlying distribution). </returns>
        Public MustOverride Function GetPDF( value As Double) As Double
        Public Overridable Function GetPeriodOfRecord() As Integer
            Return _PeriodOfRecord
        End Function
        Public MustOverride Function Validate() As List(Of ContinuousDistributionError)
        Public Sub SetPeriodOfRecord( POR As Integer)
            _PeriodOfRecord = POR
        End Sub
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
            Next i
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
                    Select Case flds(i).Type.Name
                        Case "double"
                            ParamVals(i) = flds(i).getDouble(Me)
                        Case "int"
                            ParamVals(i) = flds(i).getInt(Me)
                            'JAVA TO VB CONVERTER TODO TASK: VB does not allow fall-through from a non-empty 'case':
                        Case Else
                    End Select
                    'JAVA TO VB CONVERTER TODO TASK: There is no equivalent in VB to Java 'multi-catch' syntax:
                Catch ex As Exception 'System.ArgumentException Or IllegalAccessException ex
                    ' java.util.logging.Logger.getLogger(GetType(ContinuousDistribution).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
                End Try
            Next i
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
                    Select Case f.Type.Name
                        Case "double"
                            f.set(Dist, f.getDouble(Me))
                        Case "int"
                            f.set(Dist, f.getInt(Me))
                        Case Else
                    End Select
                Next f
                'JAVA TO VB CONVERTER TODO TASK: There is no equivalent in VB to Java 'multi-catch' syntax:
                '	Catch ClassNotFoundException Or NoSuchMethodException Or SecurityException Or InstantiationException Or IllegalAccessException Or System.ArgumentException Or InvocationTargetException ex
                'java.util.logging.Logger.getLogger(GetType(ContinuousDistribution).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
            Catch ex As Exception

            End Try
            Return Dist
        End Function
        Public Overrides Function Equals( dist As Object) As Boolean
            If dist.GetType().Name.Equals(Me.GetType().Name) Then
                Dim thisParamValues As Object() = Me.GetParamValues()
                Dim those As ContinuousDistribution = CType(dist, ContinuousDistribution)
                Dim thoseParamValues As Object() = those.GetParamValues()
                If thisParamValues.Length = thoseParamValues.Length Then
                    For i As Integer = 0 To thisParamValues.Length - 1
                        If thisParamValues(i) IsNot thoseParamValues(i) Then Return False
                    Next i
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
        'Public Shared Function ReadFromXML( ele As Element) As ContinuousDistribution
        '		Dim Dist As ContinuousDistribution = Nothing
        '		Dim c As Type
        '          Try
        '              Dim DistName As String = ele.TagName
        '              If DistName.Equals("None") Then
        '                  ' none is not supported.
        '                  Throw New System.ArgumentException
        '              ElseIf DistName.Contains(".") Then
        '                  'do nothing, this is probably from Statistics.jar.
        '              Else
        '                  If DistName.Chars(0) = "L".ToCharArray()(0) AndAlso DistName.Chars(1) <> "o".ToCharArray()(0) Then 'is l but isnt lo, so LNormal (which is how Statistics differntiates Linear moments.
        '                      DistName = "Distributions.LinearMoments." & DistName.Substring(1, DistName.Length - 1 - 1) 'remove the L.
        '                  Else
        '                      DistName = "Distributions.MethodOfMoments." & DistName
        '                  End If
        '              End If
        '              c = Type.GetType(DistName)
        '              Dist = CType(c.GetConstructor().newInstance(), ContinuousDistribution)
        '              Dim flds As Field() = c.DeclaredFields
        '              For Each f As Field In flds
        '                  Select Case f.Type.Name
        '                      Case "double"
        '                          f.set(Dist, Convert.ToDouble(ele.getAttribute(f.Name)))
        '                      Case "int"
        '                          f.set(Dist, Convert.ToInt32(ele.getAttribute(f.Name)))
        '                      Case Else
        '                          'throw error?
        '                  End Select
        '              Next f
        '              'JAVA TO VB CONVERTER TODO TASK: There is no equivalent in VB to Java 'multi-catch' syntax:
        '          Catch ex As Exception 'ClassNotFoundException Or NoSuchMethodException Or SecurityException Or InstantiationException Or IllegalAccessException Or System.ArgumentException Or InvocationTargetException ex
        '              '    java.util.logging.Logger.getLogger(GetType(ContinuousDistribution).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
        '          End Try
        '	Return Dist
        'End Function
        '		Public Overridable Function WriteToXML() As Element
        '		   Dim flds As Field() = Me.GetType().DeclaredFields
        '			Dim d As javax.xml.parsers.DocumentBuilderFactory = javax.xml.parsers.DocumentBuilderFactory.newInstance()
        '			Dim Db As javax.xml.parsers.DocumentBuilder
        '			Try
        '				Db = d.newDocumentBuilder()
        '				Dim doc As Document = Db.newDocument()
        '				Dim ele As Element = doc.createElement(Me.GetType().Name)
        '				For Each f As Field In flds
        '				Try
        '					Select Case f.Type.Name
        '						Case "double"
        '							ele.setAttribute(f.Name,Convert.ToString(f.getDouble(Me)))
        '						Case "int"
        '							ele.setAttribute(f.Name,Convert.ToString(f.getInt(Me)))
        '						Case Else
        '					End Select
        ''JAVA TO VB CONVERTER TODO TASK: There is no equivalent in VB to Java 'multi-catch' syntax:
        '					Catch System.ArgumentException Or IllegalAccessException ex
        '						java.util.logging.Logger.getLogger(GetType(ContinuousDistribution).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
        '					End Try
        '				Next f
        '				   Return ele
        '			Catch ex As javax.xml.parsers.ParserConfigurationException
        '				java.util.logging.Logger.getLogger(GetType(ContinuousDistribution).Name).log(java.util.logging.Level.SEVERE,Nothing, ex)
        '			End Try
        '			Return Nothing
        '		End Function
        ' </editor-fold>
        Public Overridable Function BootStrap() As Double()
            Dim result As Double() = New Double(_PeriodOfRecord - 1) {}
            Dim Random As New Random
            For i As Integer = 0 To _PeriodOfRecord - 1
                result(i) = GetInvCDF(Random.NextDouble())
            Next i
            Return result
        End Function
        Public Overridable Function BootStrap( seed As Long) As Double()
            Dim result As Double() = New Double(_PeriodOfRecord - 1) {}
            Dim Random As New Random(seed)
            For i As Integer = 0 To _PeriodOfRecord - 1
                result(i) = GetInvCDF(Random.NextDouble())
            Next i
            Return result
        End Function
    End Class

End Namespace

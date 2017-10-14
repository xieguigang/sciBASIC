#Region "Microsoft.VisualBasic::1c86877284f35c6abb24f6f12e8753a3, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\ApplicationDetails.vb"

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

Imports System.Reflection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices

    ''' <summary>
    ''' Parsing product assembly meta data
    ''' </summary>
    ''' <remarks>
    ''' http://www.c-sharpcorner.com/uploadfile/ravesoft/access-assemblyinfo-file-and-get-product-informations/
    ''' </remarks>
    Public Class ApplicationDetails

        Public ReadOnly Property Assembly As Assembly

        Sub New(assm As Assembly)
            Assembly = assm
            CompanyName = GetCompanyName(assm)
            ProductVersion = GetProductVersion(assm)
            ProductName = GetProductName(assm)
            CopyRightsDetail = GetCopyRightsDetail(assm)
            ProductTitle = GetProductTitle(assm)
            ProductDescription = GetProductDescription(assm)
        End Sub

        ''' <summary>
        ''' 获取当前的应用程序的描述信息
        ''' </summary>
        Sub New()
            Call Me.New(GetType(ApplicationDetails).Assembly)
        End Sub

        ''' <summary>
        ''' 获取当前进程的exe文件的程序描述信息，直接使用New申明得到的只是运行时核心模块dll文件的信息
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function CurrentExe() As ApplicationDetails
            Try
                Return New ApplicationDetails(Assembly.LoadFile(App.ExecutablePath))
            Catch ex As Exception
                ex = New Exception(App.ExecutablePath, ex)
                Call App.LogException(ex)
                Return Nothing
            End Try
        End Function

        Public Shared Function FromTypeModule(type As Type) As ApplicationDetails
            Return New ApplicationDetails(type.Assembly)
        End Function

        Public Shared Function GetCompanyName(assm As Assembly) As String
            If assm IsNot Nothing Then
                Dim companyName As String = ""
                Dim customAttributes As Object() = assm.GetCustomAttributes(GetType(AssemblyCompanyAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    companyName = DirectCast(customAttributes(0), AssemblyCompanyAttribute).Company
                End If
                If String.IsNullOrEmpty(companyName) Then
                    companyName = String.Empty
                End If

                Return companyName
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the name of the system provider name from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CompanyName() As String

        Public Shared Function GetProductVersion(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productVersion As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyVersionAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productVersion = DirectCast(customAttributes(0), AssemblyVersionAttribute).Version
                End If
                If String.IsNullOrEmpty(productVersion) Then
                    productVersion = String.Empty
                End If
                Return productVersion
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get System version from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductVersion() As String

        Public Shared Function GetProductName(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productName As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyProductAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productName = DirectCast(customAttributes(0), AssemblyProductAttribute).Product
                End If
                If String.IsNullOrEmpty(productName) Then
                    productName = String.Empty
                End If
                Return productName
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the name of the System from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductName() As String

        Public Shared Function GetCopyRightsDetail(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim copyrightsDetail As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyCopyrightAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    copyrightsDetail = DirectCast(customAttributes(0), AssemblyCopyrightAttribute).Copyright
                End If
                If String.IsNullOrEmpty(copyrightsDetail) Then
                    copyrightsDetail = String.Empty
                End If
                Return copyrightsDetail
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the copyRights details from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CopyRightsDetail() As String

        Public Shared Function GetProductTitle(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productTitle As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyTitleAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productTitle = DirectCast(customAttributes(0), AssemblyTitleAttribute).Title
                End If
                If String.IsNullOrEmpty(productTitle) Then
                    productTitle = String.Empty
                End If
                Return productTitle
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the Product tile from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductTitle() As String

        Public Shared Function GetProductDescription(assembly As Assembly) As String
            If assembly IsNot Nothing Then
                Dim productDescription As String = ""
                Dim customAttributes As Object() = assembly.GetCustomAttributes(GetType(AssemblyDescriptionAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    productDescription = DirectCast(customAttributes(0), AssemblyDescriptionAttribute).Description
                End If
                If String.IsNullOrEmpty(productDescription) Then
                    productDescription = String.Empty
                End If
                Return productDescription
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the description of the product from the assembly
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProductDescription() As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace

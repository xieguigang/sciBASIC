#Region "Microsoft.VisualBasic::2123247c2dbf0e64ee9e4932a216e17b, ApplicationServices\VBDev\ApplicationInfoUtils.vb"

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

    '     Module ApplicationInfoUtils
    ' 
    '         Function: CurrentExe, FromAssembly, FromTypeModule, GetCompanyName, GetCopyRightsDetail
    '                   GetGuid, GetProductDescription, GetProductName, GetProductTitle, GetProductVersion
    '                   VBCore
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

Namespace ApplicationServices.Development

    ''' <summary>
    ''' Parsing product assembly meta data
    ''' </summary>
    ''' <remarks>
    ''' http://www.c-sharpcorner.com/uploadfile/ravesoft/access-assemblyinfo-file-and-get-product-informations/
    ''' </remarks>
    Public Module ApplicationInfoUtils

        <Extension>
        Public Function FromAssembly(assm As Assembly) As AssemblyInfo
            Return New AssemblyInfo With {
                .AssemblyCompany = GetCompanyName(assm),
                .AssemblyFileVersion = GetProductVersion(assm),
                .AssemblyProduct = GetProductName(assm),
                .AssemblyCopyright = GetCopyRightsDetail(assm),
                .AssemblyTitle = GetProductTitle(assm),
                .AssemblyDescription = GetProductDescription(assm),
                .Guid = GetGuid(assm),
                .AssemblyVersion = assm.GetVersion().ToString
            }
        End Function

        ''' <summary>
        ''' 获取对当前运行时环境的描述
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function VBCore() As AssemblyInfo
            Return GetType(ApplicationInfoUtils).Assembly.FromAssembly
        End Function

        ''' <summary>
        ''' 获取当前进程的exe文件的程序描述信息，直接使用New申明得到的只是运行时核心模块dll文件的信息
        ''' </summary>
        ''' <returns></returns>
        Public Function CurrentExe() As AssemblyInfo
            Try
                Return Assembly.LoadFile(App.ExecutablePath).FromAssembly
            Catch ex As Exception
                ex = New Exception(App.ExecutablePath, ex)
                Call App.LogException(ex)
                Return Nothing
            End Try
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function FromTypeModule(type As Type) As AssemblyInfo
            Return type.Assembly.FromAssembly
        End Function

        Public Function GetGuid(assm As Assembly) As String
            If assm IsNot Nothing Then
                Dim attrs = assm.GetCustomAttributes(GetType(GuidAttribute), False)

                If attrs.IsNullOrEmpty Then
                    Return ""
                Else
                    Return DirectCast(attrs(Scan0), GuidAttribute).Value
                End If
            Else
                Return ""
            End If
        End Function

        ''' <summary>
        ''' Get the name of the system provider name from the assembly
        ''' </summary>
        ''' <param name="assm"></param>
        ''' <returns></returns>
        Public Function GetCompanyName(assm As Assembly) As String
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
        ''' Get System version from the assembly
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <returns></returns>
        Public Function GetProductVersion(assembly As Assembly) As String
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
        ''' Get the name of the System from the assembly
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <returns></returns>
        Public Function GetProductName(assembly As Assembly) As String
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
        ''' Get the copyRights details from the assembly
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <returns></returns>
        Public Function GetCopyRightsDetail(assembly As Assembly) As String
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
        ''' Get the Product tile from the assembly
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <returns></returns>
        Public Function GetProductTitle(assembly As Assembly) As String
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
        ''' Get the description of the product from the assembly
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <returns></returns>
        Public Function GetProductDescription(assembly As Assembly) As String
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
    End Module
End Namespace

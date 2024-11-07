#Region "Microsoft.VisualBasic::305fdf5b50ac8b63d3d185dad07fce4f, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\ApplicationInfoUtils.vb"

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

    '   Total Lines: 355
    '    Code Lines: 242 (68.17%)
    ' Comment Lines: 80 (22.54%)
    '    - Xml Docs: 88.75%
    ' 
    '   Blank Lines: 33 (9.30%)
    '     File Size: 15.12 KB


    '     Module ApplicationInfoUtils
    ' 
    '         Function: CalculateCompileTime, CurrentExe, FromAssembly, FromTypeModule, GetCompanyName
    '                   GetCopyRightsDetail, GetGuid, GetInformationalVersion, GetProductDescription, GetProductName
    '                   GetProductTitle, GetProductVersion, GetTargetFramework, GetTrademark, RetrieveLinkerTimestamp
    '                   tryGetVersion, VBCore
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports System.Runtime.Versioning
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Namespace ApplicationServices.Development

    ''' <summary>
    ''' Parsing product assembly meta data
    ''' </summary>
    ''' <remarks>
    ''' http://www.c-sharpcorner.com/uploadfile/ravesoft/access-assemblyinfo-file-and-get-product-informations/
    ''' </remarks>
    Public Module ApplicationInfoUtils

        ''' <summary>
        ''' Linker Timestamp
        ''' </summary>
        ''' 
        <Extension>
        Public Function RetrieveLinkerTimestamp(assembly As Assembly) As DateTime
            ' from stackoverflow
            Dim filePath As String = assembly.Location

            Const c_PeHeaderOffset As Integer = 60
            Const c_LinkerTimestampOffset As Integer = 8

            Dim b As Byte() = New Byte(2047) {}
            Dim s As Stream = Nothing

            Try
                s = New FileStream(filePath, FileMode.Open, FileAccess.Read)
                s.Read(b, 0, 2048)
            Finally
                If s IsNot Nothing Then
                    s.Close()
                End If
            End Try

            Dim i As Integer = BitConverter.ToInt32(b, c_PeHeaderOffset)
            Dim secondsSince1970 As Integer = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset)
            Dim dt As New DateTime(1970, 1, 1, 0, 0, 0)

            dt = dt.AddSeconds(secondsSince1970)
            dt = dt.AddHours(TimeZoneInfo.Local.GetUtcOffset(dt).Hours)

            Return dt
        End Function

        ''' <summary>
        ''' 计算出模块文件的编译时间.(在编译项目之前应该手动修改vbproj文件中的``Deterministic``配置项的值为False来允许自动递增版本号的特性)
        ''' </summary>
        ''' <param name="assm"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' ### Get Compile date and time in application 
        ''' 
        ''' > https://social.msdn.microsoft.com/forums/en-US/172201e0-c47b-40a8-a5d7-0a052cb42532/get-compile-date-and-time-in-application
        ''' 
        ''' Set up your Build and MinorRevision numbers to auto-increment, pull out the date created for 
        ''' by using the Version numbers from the version on the class. 
        ''' 
        ''' 1. Open ``AssemblyInfo.vb``.  You'll find it under the Properties folder in your solution.  
        ''' Find the two lines that say "AssemblyVersion" and "AssemblyFileVersion" in them.  
        ''' Change them to:
        '''
        ''' ```vbnet
        ''' &lt;assembly AssemblyVersion("1.0.*")>
        ''' &lt;assembly AssemblyFileVersion("1.0.*")>
        ''' ```
        '''
        ''' Once you Do this, Visual Studio will automatically increment the last two values In the version.  
        ''' 
        ''' 2. The creation date can be found at runtime using the following algorithm:
        '''
        ''' ```vbnet
        ''' Dim version = Assembly.GetExecutingAssembly().GetName().Version
        ''' Dim creationDate = New DateTime(2000, 1, 1).AddDays(version.Build).AddSeconds(version.MinorRevision * 2)
        ''' ```
        ''' </remarks>
        <Extension>
        Public Function CalculateCompileTime(assm As Assembly) As Date
            If assm Is Nothing Then
                Return Nothing
            Else
                Dim version As Version = assm.GetName.Version
                Dim builtTime = New DateTime(2000, 1, 1) _
                    .AddDays(version.Build) _
                    .AddSeconds(version.MinorRevision * 2)

                Return builtTime
            End If
        End Function

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
                .AssemblyVersion = assm.tryGetVersion.ToString,
                .BuiltTime = assm.CalculateCompileTime,
                .AssemblyFullName = any.ToString(assm.GetName),
                .AssemblyInformationalVersion = GetInformationalVersion(assm),
                .AssemblyTrademark = GetTrademark(assm),
                .TargetFramework = GetTargetFramework(assm),
                .Name = assm.GetName?.Name
            }
        End Function

        <Extension>
        Private Function tryGetVersion(assm As Assembly) As Version
            Try
                Return assm.FullName _
                    .Match("Version[=]\s*\S+,") _
                    .Trim(",") _
                    .GetTagValue("="c) _
                    .Value _
                    .DoCall(Function(ver) Version.Parse(ver))
            Catch ex As Exception
                Return New Version
            End Try
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

        Public Function GetTargetFramework(assm As Assembly) As String
            If assm IsNot Nothing Then
                Dim targetFramework As String = ""
                Dim customAttributes As Object() = assm.GetCustomAttributes(GetType(TargetFrameworkAttribute), False)
                If (customAttributes IsNot Nothing) AndAlso (customAttributes.Length > 0) Then
                    targetFramework = DirectCast(customAttributes(0), TargetFrameworkAttribute).FrameworkName
                End If
                If String.IsNullOrEmpty(targetFramework) Then
                    targetFramework = String.Empty
                End If

                Return targetFramework
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

        Public Function GetTrademark(assm As Assembly) As String
            If assm IsNot Nothing Then
                Dim trademark As String = ""
                Dim customAttributes As Object() = assm.GetCustomAttributes(GetType(AssemblyTrademarkAttribute), False)

                If customAttributes IsNot Nothing AndAlso customAttributes.Length > 0 Then
                    trademark = DirectCast(customAttributes(Scan0), AssemblyTrademarkAttribute).Trademark
                End If
                If String.IsNullOrEmpty(trademark) Then
                    trademark = String.Empty
                End If

                Return trademark
            Else
                Return ""
            End If
        End Function

        Public Function GetInformationalVersion(assm As Assembly) As String
            If assm IsNot Nothing Then
                Dim infoVer As String = ""
                Dim customAttributes As Object() = assm.GetCustomAttributes(GetType(AssemblyInformationalVersionAttribute), False)

                If customAttributes IsNot Nothing AndAlso customAttributes.Length > 0 Then
                    infoVer = DirectCast(customAttributes(Scan0), AssemblyInformationalVersionAttribute).InformationalVersion
                End If
                If String.IsNullOrEmpty(infoVer) Then
                    infoVer = String.Empty
                End If

                Return infoVer
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

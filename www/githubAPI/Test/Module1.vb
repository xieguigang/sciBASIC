#Region "Microsoft.VisualBasic::8c82fa439c2a50b3e1adce094578398f, www\githubAPI\Test\Module1.vb"

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

    ' Module Module1
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports Microsoft.VisualBasic.Webservices.Github.Visualizer
Imports Microsoft.VisualBasic.Webservices.Github.WebAPI

Module Module1

    Sub Main()

        'Dim my As User = WebAPI.Users.GetUserData("xieguigang")

        WebServiceUtils.Proxy = "http://127.0.0.1:8087"

        ''   Call "xieguigang".GetUserContributions .GetJson .SaveTo ("x:\xieguigang_contributions.json")

        'Call IsometricContributions.Plot(
        '    "xieguigang".GetUserContributions,
        '    schema:="Spectral:c8", user:=my).Save("G:\GCModeller\src\runtime\sciBASIC#\www\data\github\xieguigang_contributions.png")

        'Pause()

        'Call IsometricContributions.Plot("xieguigang").Save("x:\text.png")

        'Pause()

        'Dim contributions = "xieguigang".GetUserContributions


        Dim followers As User() = WebAPI.Users.Followers("xieguigang") '= "J:\GCModeller\src\runtime\sciBASIC#\www\data\github\followers.json".ReadAllText.LoadObject(Of User()) '"xieguigang".Followers
        Call followers.GetJson(True).SaveTo("./followers.json")

        Dim following As User() = WebAPI.Users.Following("xieguigang")  '"xieguigang".Following
        Call following.GetJson(True).SaveTo("./following.json")

        Dim notFollings = following.WhoIsNotFollowMe(followers).ToArray
        Call notFollings.GetJson(True).SaveTo("./notfollowing.json")

        Dim IamNotFollowings = following.WhoIamNotFollow(followers).ToArray
        Call IamNotFollowings.GetJson(True).SaveTo("./I-am-not-Following.json")

        Pause()
    End Sub
End Module

Imports Microsoft.VisualBasic.Webservices.Github.API
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Webservices.Github

Module Module1

    Sub Main()
        Dim followers As User() = "J:\GCModeller\src\runtime\sciBASIC#\www\data\github\followers.json".ReadAllText.LoadObject(Of User()) '"xieguigang".Followers
        '   Call followers.GetJson.SaveTo("./followers.json")

        Dim following As User() = "J:\GCModeller\src\runtime\sciBASIC#\www\data\github\following.json".ReadAllText.LoadObject(Of User()) '"xieguigang".Following
        ' Call following.GetJson.SaveTo("./following.json")

        Dim notFollings = following.WhoIsNotFollowMe(followers).ToArray
        Call notFollings.GetJson.SaveTo("./notfollowing.json")

    End Sub
End Module

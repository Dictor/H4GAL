Imports System.IO
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text
Imports Newtonsoft.Json.Linq

Module KakaoAuth
    Private APIadminKey As String = Nothing
    Private Const APIhost As String = "kapi.kakao.com"

    Private Const APIuserInfo_uri As String = "/v2/user/me"
    Private Const APIuserInfo_auth As String = "Bearer "
    Private Const APIuserInfo_contenType As String = "application/x-www-form-urlencoded;charset=utf-8"

    Private userDBPath As String
    Private userDB As Dictionary(Of String, Dictionary(Of String, String))
    Private userRegisterCode As String()

    Public Sub Init()
        APIadminKey = File.ReadAllText(EngineWrapper.EngineArgument.ApplicationStartupPath & "\data\kakaoapi_adminkey.txt")
        userDBPath = EngineWrapper.EngineArgument.ApplicationStartupPath & "\data\kakaoapi_user.db"
        userDB = ReadMulti(userDBPath)
        userRegisterCode = Split(File.ReadAllText(EngineWrapper.EngineArgument.ApplicationStartupPath & "\data\kakaoapi_registercode.txt"))
    End Sub

    Public Async Function GetTokenInfo(token As String) As Task(Of JObject)
        Dim nowReq As WebRequest = WebRequest.Create(APIhost & APIuserInfo_uri)
        Dim nowContent As HttpWebRequest = CType(nowReq, HttpWebRequest)
        nowContent.PreAuthenticate = True
        nowContent.Headers.Add(HttpRequestHeader.Authorization, APIuserInfo_auth & token)
        nowContent.ContentType = APIuserInfo_contenType
        Dim getResp As Task(Of WebResponse) = nowReq.GetResponseAsync()
        Dim nowResp As WebResponse = Await getResp
        Dim respStream As New StreamReader(nowResp.GetResponseStream, Encoding.Default)
        Dim respString As String = respStream.ReadToEnd
        Dim respJson As JObject = JObject.Parse(respString)
        respStream.Close()
        Return respJson
    End Function

    Public Function UDB_hasUser(uid As String) As Boolean
        Return userDB.ContainsKey(uid)
    End Function

    Public Function UDB_getUserInfo(uid As String) As Dictionary(Of String, String)
        Return userDB(uid)
    End Function

    Public Sub UDB_save()
        Write(userDBPath, userDB)
        userDB = ReadMulti(userDBPath)
    End Sub

    Public Sub UDB_setUser(uid As String, userinfo As Dictionary(Of String, String))
        userDB.Add(uid, userinfo)
    End Sub

    Public Function CheckRegisterCode(codehash As String)
        For Each nowcode In userRegisterCode
            If SHA256Hash(nowcode + nowcode.Length.ToString) = codehash Then
                Return True
            End If
        Next
        Return False
    End Function

    Public Function SHA256Hash(ByVal data As String) As String
        Dim sha As SHA256 = New SHA256Managed()
        Dim hash As Byte() = sha.ComputeHash(Encoding.ASCII.GetBytes(data))
        Dim stringBuilder As StringBuilder = New StringBuilder()
        For Each b As Byte In hash
            stringBuilder.AppendFormat("{0:x2}", b)
        Next
        Return stringBuilder.ToString()
    End Function
End Module


Imports System.Drawing
Imports System.IO
Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms
Imports Newtonsoft.Json.Linq

Public Class frmMain
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Me.Shown, AddressOf initproc
    End Sub

    Private ServerSoc As Object
    Delegate Sub SocCb(kind As String, args As Object())
    Private clichecktim As New System.Timers.Timer(1000)
    Private DispAuthCodelist As String()
    Private SessionList As New Dictionary(Of String, Session)
    Private totalSentByte As Long = 0
    Private totalHandshake As Long = 0
    Private totalAccept As Long = 0
    Private socnumToIp As New Dictionary(Of String, String)

    Private Const sessionExpireMinute As Integer = 30

    Private Class Session
        Public SocketNumber As Integer
        Public SessionStatus As SessionFlag = SessionFlag.NoCredential
        Public CredentialUserName As String
        Public CredentialStartTime As Date
        Public otherData As New Dictionary(Of String, String)
        Public Enum SessionFlag
            NoCredential
            DisposableCredential
            AccountCredential
            KakaoCredential
        End Enum
    End Class

    <Flags()>
    Enum ErrorModes As UInteger
        SYSTEM_DEFAULT = 0
        SEM_FAILCRITICALERRORS = 1
        SEM_NOALIGNMENTFAULTEXCEPT = 4
        SEM_NOGPFAULTERRORBOX = 2
        SEM_NOOPENFILEERRORBOX = 32768
    End Enum

    Class NativeMethods
        Friend Declare Function SetErrorMode Lib "kernel32.dll" (ByVal mode As ErrorModes) As ErrorModes
    End Class

    Public Sub initproc()
        Print("INIT", "", "UI 로드 완료")
        lstLog_SizeChanged(Nothing, Nothing)
        NativeMethods.SetErrorMode((ErrorModes.SEM_NOGPFAULTERRORBOX) Or (ErrorModes.SEM_FAILCRITICALERRORS Or ErrorModes.SEM_NOOPENFILEERRORBOX))
        AddHandler Application.ThreadException, New Threading.ThreadExceptionEventHandler(Sub(sender As Object, info As Threading.ThreadExceptionEventArgs)
                                                                                              EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("Application.ThreadException", "핸들되지 않은 예외 발생, 엔진 종료")
                                                                                              EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("Application.ThreadException", info.Exception.ToString)
                                                                                              EngineWrapper.EngineFunction.EFUNC_EngineShutdown.DynamicInvoke()
                                                                                          End Sub)
        AddHandler AppDomain.CurrentDomain.UnhandledException, New UnhandledExceptionEventHandler(Sub(sender As Object, info As UnhandledExceptionEventArgs)
                                                                                                      EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("AppDomain.CurrentDomain.UnhandledException", "핸들되지 않은 예외 발생, 엔진 종료")
                                                                                                      EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("AppDomain.CurrentDomain.UnhandledException", info.ExceptionObject.ToString)
                                                                                                      EngineWrapper.EngineFunction.EFUNC_EngineShutdown.DynamicInvoke()
                                                                                                  End Sub)
        Print("INIT", "", "윈도우 오류 다이얼로그 비활성 완료")
        Print("INIT", "", Project.Version.GetName & "  " & Project.Version.GetVersion(True))
        Try
            ServerSoc = Project.HEsock.CopyMe()
            Print("INIT", "", "0.0.0.0:81에서 Listen모드로 소켓을 초기화합니다.")
            ServerSoc.init(True, "0.0.0.0", 81, New SocCb(AddressOf SocCallback))
            'AddHandler clichecktim.Elapsed, AddressOf chkcli
            Print("INIT", "", "Listen을 시작합니다")
            ServerSoc.SetListen()
            'clichecktim.Start()
            Print("INIT", "", "소켓 시작 작업 완료")
            Dim fileopener = Project.HEfile.CopyMe()
            fileopener.SetFile(Application.StartupPath & "\data\allowDispAuthCode.txt", Encoding.UTF8)
            DispAuthCodelist = Split(fileopener.ReadText(), vbCrLf)
            Print("INIT", "", "엑세스 코드 리스트 읽기 완료")
            AddHandler clichecktim.Elapsed, AddressOf CheckSession
            clichecktim.AutoReset = True
            clichecktim.Start()
            Print("INIT", "", "세션 검사 타이머 시작 완료")
            KakaoAuth.Init()
            Print("INIT", "", "카카오 API 초기화 완료")
            Print("INIT", "", "썸네일 검사 시작")
            MakeThumbNail()
            Print("INIT", "", "썸네일 검사 완료")
        Catch ex As Exception
            Print("ERROR", "", "소켓을 초기화하는중 오류가 발생했습니다!!")
            Print("ERROR", "", ex.ToString)
        End Try
    End Sub

    Private Sub CheckSession()
        txtAvailSess.Text = "활성 세션: " & SessionList.Count.ToString
        For Each nowSess In SessionList
            If (Now - nowSess.Value.CredentialStartTime).TotalMinutes > sessionExpireMinute Then
                Print("CHECKSESSION", "", "SID : '" & nowSess.Key & "'세션 만료")
                SessionList.Remove(nowSess.Key)
            End If
        Next
    End Sub

    Public Sub SocCallback(kind As String, args As Object())
        If kind = "ACCEPT" Then
            Dim clisoc As Socket = args(0)
            EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("SocCallback", "[ACCEPT]" & clisoc.RemoteEndPoint.ToString & " -> " & args(1))
            If socnumToIp.ContainsKey(args(1)) Then
                socnumToIp.Add(args(1), clisoc.RemoteEndPoint.ToString)
            Else
                socnumToIp(args(1)) = clisoc.RemoteEndPoint.ToString
            End If
            totalAccept += 1
                txtTotalAccept.Text = "총 소켓 Accept: " & totalAccept.ToString
            ElseIf kind = "RECEIVE" Then
                ProcessMsg(args(2), args(1))
            ElseIf kind = "SEND" Then
                EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("SocCallback", "[SEND] " & args(1) & "bytes")
                totalSentByte += Convert.ToUInt32(args(1))
                txtSentBytes.Text = "총 전송 : " & Math.Round(totalSentByte / 1000.0F, 1).ToString & "kB"
            ElseIf kind = "DISCONNECT" Then
                EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("SocCallback", "[DISCONN] " & args(0))
            ElseIf kind = "ERROR" Then
                If (CType(args(1), Exception).GetType.FullName = "System.Net.Sockets.SocketException") Or args(0) = "CFUNC_SEND" Then
                If Thread.CurrentThread.Name.Contains("SEND") Then
                    Print("ERROR", "", "SENDIMG or SENDTHIMG Abort")
                    Thread.CurrentThread.Abort()
                End If
                EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("SocCallback", "[ERROR] (" & args(2).ToString() & ")" & args(0) & " : " & args(1).ToString)
            Else
                Try
                    ServerSoc.CloseClient(args(2).ToString())
                Catch ex As Exception
                    EngineWrapper.EngineFunction.EFUNC_LogWrite.DynamicInvoke("[ERROR]Client Socket Close Fail")
                End Try
                Print("ERROR", "", "(" & args(2).ToString() & ")" & args(0) & " : " & args(1).ToString)
            End If
        End If
    End Sub

    Private Async Sub ProcessMsg(data As Byte(), socnum As Integer)
        Try
            Dim httpmsg = Encoding.UTF8.GetString(data)
            If New Regex("^GET").IsMatch(httpmsg) Then 'GET REQ시	
                Dim response As [Byte]() = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine + "Connection: Upgrade" + Environment.NewLine + "Upgrade: websocket" + Environment.NewLine + "Sec-WebSocket-Accept: " + Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(New Regex("Sec-WebSocket-Key: (.*)").Match(httpmsg).Groups(1).Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) + Environment.NewLine + Environment.NewLine)
                ServerSoc.Send(response, socnum)
                EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke("ProcessMsg", "[SOCKET]" & socnum & "번 소켓에서 웹소켓 핸드셰이크")
                totalHandshake += 1
                txtTotalHandshake.Text = "총 WS H/S: " & totalHandshake.ToString
            Else 'NON-GET REQ시
                Dim msg As String = DecodeMessage(data)
                'Print("[SOCKET]" & socnum & "번 소켓에서 데이터 수신 : '" & msg & "'")
                Dim pmsg As String() = Split(msg, "#")
                Dim reqName As String = pmsg(0)
                Dim reqdata As JObject = Nothing
                Try
                    reqdata = JObject.Parse(pmsg(1))
                Catch
                    'Print("ERROR", "", "API Request Data Json Parse Error!")
                End Try
                If reqName = "SESSION" Then
                    Dim nowsid = reqdata("sid")
                    If SessionList.ContainsKey(nowsid) Then
                        If SessionList(nowsid).SessionStatus = Session.SessionFlag.NoCredential Then
                            SendREQ("GETCREDENTIAL", New JObject From {{"isNew", False}, {"status", "empty"}}, socnum)
                        ElseIf SessionList(nowsid).SessionStatus = Session.SessionFlag.DisposableCredential Then
                            SendREQ("GETCREDENTIAL", New JObject From {{"isNew", False}, {"status", "disposable"}, {"name", SessionList(nowsid).CredentialUserName}}, socnum)
                        ElseIf SessionList(nowsid).SessionStatus = Session.SessionFlag.AccountCredential Then
                            SendREQ("GETCREDENTIAL", New JObject From {{"isNew", False}, {"status", "account"}, {"name", SessionList(nowsid).CredentialUserName}}, socnum)
                        ElseIf SessionList(nowsid).SessionStatus = Session.SessionFlag.KakaoCredential Then
                            SendREQ("GETCREDENTIAL", New JObject From {{"isNew", False}, {"status", "kakao"}, {"name", SessionList(nowsid).CredentialUserName}}, socnum)
                        End If
                    Else
                        SendREQ("GETCREDENTIAL", New JObject From {{"isNew", True}, {"error", ""}}, socnum)
                    End If
                ElseIf reqName = "MAKESESSION" Then
                    Dim sid As Guid = Guid.NewGuid()
                    Dim nowjson As New JObject From {{"sid", sid.ToString}}
                    SendREQ("ISSUESESSION", nowjson, socnum)
                    Dim nowsess = New Session
                    nowsess.SocketNumber = socnum
                    nowsess.SessionStatus = Session.SessionFlag.NoCredential
                    nowsess.CredentialStartTime = Now
                    SessionList.Add(sid.ToString, nowsess)
                    If socnumToIp.ContainsKey(socnum) Then
                        Print("ISSUESESSION", sid.ToString.Substring(0, 6), socnum & "(" & socnumToIp(socnum) & ") → " & sid.ToString)
                    Else
                        Print("ISSUESESSION", sid.ToString.Substring(0, 6), socnum & " → " & sid.ToString)
                    End If
                ElseIf reqName = "TRYKAKAOAUTH" Then
                    Dim nowsid = reqdata("sid")
                    If SessionList.ContainsKey(nowsid) Then
                        If Not SessionList(nowsid).SessionStatus = Session.SessionFlag.NoCredential Then
                            SendREQ("TRYKAKAOAUTH", New JObject From {{"status", False}, {"error", "ILLEGAL_CREDENTIAL"}}, socnum)
                            Exit Sub
                        Else
                            Dim nowToken As String = reqdata("token")
                            Dim nowResp = Await KakaoAuth.GetTokenInfo(nowToken)
                            Dim kname = nowResp("properties")("nickname"), kuid = nowResp("id")
                            If UDB_hasUser(kuid) Then
                                SessionList(nowsid).SessionStatus = Session.SessionFlag.KakaoCredential
                                SessionList(nowsid).CredentialUserName = UDB_getUserInfo(kuid)("NAME")
                                SendREQ("TRYKAKAOAUTH", New JObject From {{"status", True}}, socnum)
                                Print("TRYKAKAOAUTH", nowsid.ToString.Substring(0, 6), "'" & kname.ToString & "'(" & kuid.ToString & ") -> 토큰 '" & reqdata("token").ToString & "'")
                            Else
                                SendREQ("TRYKAKAOAUTH", New JObject From {{"status", False}, {"error", "NEED_REGISTER"}}, socnum)
                            End If
                        End If
                    Else
                        SendREQ("TRYKAKAOAUTH", New JObject From {{"status", False}, {"error", "INVALID_SESSION"}}, socnum)
                    End If
                ElseIf reqName = "REGISTERKAKAOAUTH" Then
                    Dim nowsid = reqdata("sid")
                    If SessionList.ContainsKey(nowsid) Then
                        If Not SessionList(nowsid).SessionStatus = Session.SessionFlag.NoCredential Then
                            SendREQ("REGISTERKAKAOAUTH", New JObject From {{"status", False}, {"error", "ILLEGAL_CREDENTIAL"}}, socnum)
                            Exit Sub
                        Else
                            Dim nowToken As String = reqdata("token")
                            Dim nowResp = Await KakaoAuth.GetTokenInfo(nowToken)
                            Dim kname = nowResp("properties")("nickname"), kuid = nowResp("id")
                            If UDB_hasUser(kuid) Then
                                SendREQ("REGISTERKAKAOAUTH", New JObject From {{"status", False}, {"error", "ALREADY_REGISTERED"}}, socnum)
                            Else
                                If KakaoAuth.CheckRegisterCode(reqdata("code")) Then
                                    UDB_setUser(kuid, New Dictionary(Of String, String) From {{"NAME", kname}})
                                    UDB_save()
                                    SendREQ("REGISTERKAKAOAUTH", New JObject From {{"status", True}}, socnum)
                                    Print("REGISTERKAKAOAUTH", nowsid.ToString.Substring(0, 6), "'" & kname.ToString & "'(" & kuid.ToString & ") -> 코드 '" & reqdata("code").ToString & "'")
                                Else
                                    SendREQ("REGISTERKAKAOAUTH", New JObject From {{"status", False}, {"error", "INCORRECT_CODE"}}, socnum)
                                End If
                            End If
                        End If
                    Else
                        SendREQ("REGISTERKAKAOAUTH", New JObject From {{"status", False}, {"error", "INVALID_SESSION"}}, socnum)
                    End If
                ElseIf reqName = "TRYDISPAUTH" Then
                        Dim nowsid = reqdata("sid")
                        If SessionList.ContainsKey(nowsid) Then
                            If Not SessionList(nowsid).SessionStatus = Session.SessionFlag.NoCredential Then
                                SendREQ("TRYDISPAUTH", New JObject From {{"status", False}, {"error", "ILLEGAL_CREDENTIAL"}}, socnum)
                                Exit Sub
                            End If
                            Dim nowstuid As Integer
                            If Integer.TryParse(reqdata("stuid"), nowstuid) Then
                                If Uri.UnescapeDataString(reqdata("name").ToString).Length > 5 Then
                                    SendREQ("TRYDISPAUTH", New JObject From {{"status", False}, {"error", "ILLEGAL_NAME"}}, socnum)
                                    Exit Sub
                                End If
                                If nowstuid > 30000 And nowstuid < 40000 Then
                                    Dim codecorrect As Boolean = False
                                    For Each nowcode In DispAuthCodelist
                                        If SHA256Hash(nowcode + nowcode.Length.ToString) = reqdata("code") Then
                                            codecorrect = True
                                        End If
                                    Next
                                    If codecorrect Then
                                        SendREQ("TRYDISPAUTH", New JObject From {{"status", True}}, socnum)
                                        SessionList(nowsid).CredentialUserName = Uri.UnescapeDataString(reqdata("name").ToString)
                                        SessionList(nowsid).SessionStatus = Session.SessionFlag.DisposableCredential
                                        SessionList(nowsid).otherData.Add("STUID", reqdata("stuid"))
                                        Print("DISPAUTH", nowsid.ToString.Substring(0, 6), "'" & SessionList(nowsid).CredentialUserName & "'(" & SessionList(nowsid).otherData("STUID") & ") -> 코드 '" & reqdata("code").ToString & "'")
                                    Else
                                        SendREQ("TRYDISPAUTH", New JObject From {{"status", False}, {"error", "INCORRECT_CODE"}}, socnum)
                                    End If
                                Else
                                    SendREQ("TRYDISPAUTH", New JObject From {{"status", False}, {"error", "ILLEGAL_STUID"}}, socnum)
                                End If
                            Else
                                SendREQ("TRYDISPAUTH", New JObject From {{"status", False}, {"error", "ILLEGAL_STUID"}}, socnum)
                            End If
                        Else
                            SendREQ("TRYDISPAUTH", New JObject From {{"status", False}, {"error", "INVALID_SESSION"}}, socnum)
                        End If
                    ElseIf reqName = "GETIMGLIST" Then
                        Dim nowsid = reqdata("sid")
                        If SessionList.ContainsKey(nowsid) Then
                            Dim nowpath = reqdata("dir").ToString
                            nowpath = Uri.UnescapeDataString(nowpath)
                            nowpath = Replace(nowpath, "/", "\")
                            nowpath = EscapeDirectoryPath(nowpath)
                            Dim fileopener As Object = Project.HEfile.CopyMe()
                            Print("GETIMGLIST", nowsid.ToString.Substring(0, 6), Application.StartupPath & "\image" & nowpath + "imglist.lst")
                            fileopener.SetFile(Application.StartupPath & "\image" & nowpath + "imglist.lst", Encoding.UTF8)
                            If fileopener.Exist() Then
                                Dim nowlist = Split(fileopener.ReadText(), vbCrLf)
                                'ALBUM,TEST1,테스트 앨범 제목 1,테스트 앨범 제목 2,NONE 썸네일,ALL 권한
                                Dim listtojson As New JArray
                                For Each nowele As String In nowlist
                                    Dim nowprop As String() = Split(nowele, ",")
                                    If nowprop(0) = "AUTOPHOTO" Then
                                        Dim imgfileobj As Object = Project.HEfile.CopyMe()
                                        imgfileobj.Directory.SetDirectory(Application.StartupPath & "\image" & nowpath)
                                        Dim imgfilelst As String() = imgfileobj.Directory.GetFile()
                                        For Each nowfile As String In imgfilelst
                                            If isImageFilePath(nowfile) Then
                                                listtojson.Add(New JObject From {{"type", "PHOTO"}, {"dir", nowfile.Split("\").Last}, {"title", nowfile.Split("\").Last}, {"detail", Nothing}, {"thimg", ComputeFileHash(nowfile)}, {"isautoth", True}})
                                            End If
                                        Next
                                        Exit For
                                    End If
                                    listtojson.Add(New JObject From {{"type", nowprop(0)}, {"dir", nowprop(1)}, {"title", nowprop(2)}, {"detail", nowprop(3)}, {"thimg", nowprop(4)}, {"isautoth", False}})
                                Next
                                SendREQ("GETIMGLIST", New JObject From {{"status", True}, {"result", listtojson}}, socnum)
                            Else
                                SendREQ("GETIMGLIST", New JObject From {{"status", False}, {"error", "INVALID_PATH"}}, socnum)
                            End If
                        Else
                            SendREQ("GETIMGLIST", New JObject From {{"status", False}, {"error", "INVALID_SESSION"}}, socnum)
                        End If
                    ElseIf reqName = "GETIMG" Then
                        Dim nowsid = reqdata("sid")
                        If SessionList.ContainsKey(nowsid) Then
                            Dim nowpath = reqdata("dir").ToString
                            nowpath = Uri.UnescapeDataString(nowpath)
                            nowpath = Replace(nowpath, "/", "\")
                            nowpath = EscapeDirectoryPath(nowpath)
                            Dim nowthread As Thread = New Thread(Sub()
                                                                     SendImg(nowsid, socnum, nowpath)
                                                                 End Sub)
                            nowthread.Name = "SENDIMG" & socnum.ToString
                            nowthread.Start()
                        Else
                            SendREQ("GETIMG", New JObject From {{"status", False}, {"error", "INVALID_SESSION"}}, socnum)
                        End If
                    ElseIf reqName = "GETTHUMB" Then
                        Dim nowsid = reqdata("sid")
                    If SessionList.ContainsKey(nowsid) Then
                        Dim nowpath = reqdata("thid").ToString
                        nowpath = Replace(nowpath, "/", "\")
                        nowpath = EscapeFilePath(nowpath)
                        Dim nowthread As Thread = New Thread(Sub()
                                                                 SendThImg(nowsid, socnum, nowpath)
                                                             End Sub)
                        nowthread.Name = "SENDTHIMG" & socnum.ToString
                        nowthread.Start()
                    Else
                        SendREQ("GETTHUMB", New JObject From {{"status", False}, {"error", "INVALID_SESSION"}}, socnum)
                    End If
                End If
            End If
        Catch ex As Exception
            Print("ERROR", "", ex.ToString)
            Try
                SendREQ("SHOWALERT", "INTERNAL SERVER ERROR!", socnum)
            Catch
            End Try
        End Try
    End Sub

    Public Sub SendREQ(APIname As String, APIdata As JObject, socnum As Integer)
        ServerSoc.Send(CreateFrame(APIname + "#" + APIdata.ToString), socnum)
    End Sub

    Private Sub SendImg(nowsid As String, socnum As Integer, imgdir As String)
        Dim fileopener As Object = Project.HEfile.CopyMe()
        Print("GETIMG", nowsid.Substring(0, 6), "(" & Thread.CurrentThread.GetHashCode & ")" & Application.StartupPath & "\image" & imgdir)
        fileopener.SetFile(Application.StartupPath & "\image" & imgdir, Encoding.UTF8)
        If fileopener.Exist() Then
            Dim imgbase64str As String = Convert.ToBase64String(fileopener.ReadByte())
            Const slicesize As Integer = 342 * 40 'chars, 4 base64 char = 3byte // 342*4에서 사이즈 키움
            Dim nowslice = 0, endslice = Math.Ceiling(imgbase64str.Length / slicesize)
            SendREQ("GETIMG", New JObject From {{"status", True}, {"task", "start"}, {"name", imgdir.Replace("\", "/")}, {"count", Convert.ToInt32(endslice + 1)}}, socnum)
            Dim nowstring As String
            For nowslice = 0 To endslice - 1
                Dim startaddr = nowslice * slicesize, endaddr = (nowslice + 1) * slicesize - 1
                If endaddr >= imgbase64str.Length Then
                    endaddr = imgbase64str.Length - 1
                End If
                nowstring = imgbase64str.Substring(startaddr, endaddr - startaddr + 1)
                SendREQ("GETIMG", New JObject From {{"status", True}, {"task", "slice"}, {"name", imgdir.Replace("\", "/")}, {"slicecount", nowslice}, {"data", nowstring}}, socnum)
            Next
            SendREQ("GETIMG", New JObject From {{"status", True}, {"task", "end"}, {"name", imgdir.Replace("\", "/")}}, socnum)
        Else
            SendREQ("GETIMG", New JObject From {{"status", False}, {"error", "INVALID_PATH"}}, socnum)
        End If
    End Sub

    Private Sub SendThImg(nowsid As String, socnum As Integer, thid As String)
        Try
            Dim fileopener As Object = Project.HEfile.CopyMe()
            Print("GETTHUMB", nowsid.Substring(0, 6), "THID : " & thid)
            fileopener.Directory.SetDirectory(Application.StartupPath & "\thumb\")
            Dim resfile As String = ""
            For Each nowf As String In fileopener.Directory.GetFile()
                If nowf.Split("\").Last.Contains(thid) Then
                    resfile = nowf
                End If
            Next
            If resfile = "" Then
                SendREQ("GETTHUMB", New JObject From {{"status", False}, {"error", "INVALID_THID"}}, socnum)
                Exit Sub
            End If
            fileopener.SetFile(resfile, Encoding.UTF8)

            If fileopener.Exist() Then
                Dim imgbase64str As String = Convert.ToBase64String(fileopener.ReadByte())
                Const slicesize As Integer = 342 * 40 'chars, 4 base64 char = 3byte
                Dim nowslice = 0, endslice = Math.Ceiling(imgbase64str.Length / slicesize)
                SendREQ("GETTHUMB", New JObject From {{"status", True}, {"task", "start"}, {"name", thid}}, socnum)
                Dim nowstring As String
                For nowslice = 0 To endslice - 1
                    Dim startaddr = nowslice * slicesize, endaddr = (nowslice + 1) * slicesize - 1
                    If endaddr >= imgbase64str.Length Then
                        endaddr = imgbase64str.Length - 1
                    End If
                    nowstring = imgbase64str.Substring(startaddr, endaddr - startaddr + 1)
                    SendREQ("GETTHUMB", New JObject From {{"status", True}, {"task", "slice"}, {"name", thid}, {"slicecount", nowslice}, {"data", nowstring}}, socnum)
                Next
                SendREQ("GETTHUMB", New JObject From {{"status", True}, {"task", "end"}, {"name", thid}}, socnum)
            Else
                SendREQ("GETTHUMB", New JObject From {{"status", False}, {"error", "INVALID_PATH"}}, socnum)
            End If
        Catch ex As Exception
            Print("ERROR", "", ex.ToString)
            Try
                SendREQ("SHOWALERT", "INTERNAL SERVER ERROR!", socnum)
            Catch
            End Try
        End Try
    End Sub

    Public Sub Print(kind As String, user As String, data As String)
        Try
            Dim nowrow As New ListViewItem({Now.ToString("MM/dd HH:mm:ss"), kind, user, data})
            lstLog.Items.Add(nowrow)
            lstLog.EnsureVisible(lstLog.Items.Count - 1)
            EngineWrapper.EngineFunction.EFUNC_LogWriteP.DynamicInvoke(kind, "(" & user & ")" & data)
        Catch
        End Try
    End Sub

    Private Sub bthHalt_Click(sender As Object, e As EventArgs) Handles bthHalt.Click
        Try
            ServerSoc.Shutdown()
        Finally
            EngineWrapper.EngineFunction.EFUNC_EngineShutdown.DynamicInvoke()
        End Try
    End Sub

    Private Sub lstLog_SizeChanged(sender As Object, e As EventArgs) Handles lstLog.SizeChanged
        lstLog.Columns.Item(3).Width = lstLog.Width - (lstLog.Columns.Item(0).Width + lstLog.Columns.Item(1).Width + lstLog.Columns.Item(2).Width)
    End Sub

    Public Shared Function EscapeFilePath(ByVal s As String) As String
        Dim regex As Regex = New Regex(String.Format("[{0}]", Regex.Escape(New String(Path.GetInvalidFileNameChars()))))
        s = regex.Replace(s, "")
        Return s
    End Function

    Public Shared Function EscapeDirectoryPath(ByVal s As String) As String
        Dim regex As Regex = New Regex(String.Format("[{0}]", Regex.Escape(New String(Path.GetInvalidPathChars()))))
        s = regex.Replace(s, "")
        Return s
    End Function
End Class
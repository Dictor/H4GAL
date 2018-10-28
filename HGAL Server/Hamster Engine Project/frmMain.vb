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
    Private clichecktim As New System.Timers.Timer(100)
    Private DispAuthCodelist As String()
    Private totalSentByte As Long = 0

    Private SessionList As New Dictionary(Of String, Session)
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
        Print("[INIT]UI 로드 완료")
        NativeMethods.SetErrorMode((NativeMethods.SetErrorMode(0) _
                Or (ErrorModes.SEM_NOGPFAULTERRORBOX _
                Or (ErrorModes.SEM_FAILCRITICALERRORS Or ErrorModes.SEM_NOOPENFILEERRORBOX))))
        Print("[INIT]윈도우 오류 다이얼로그 비활성 완료")

        Print("[INIT]" & Project.Version.GetName & "  " & Project.Version.GetVersion(True))
        Try
            ServerSoc = Project.Hamsoc.CopyMe()
            Print("[INIT]0.0.0.0:81에서 Listen모드로 소켓을 초기화합니다.")
            ServerSoc.init(True, "0.0.0.0", 81, New SocCb(AddressOf SocCallback))
            'AddHandler clichecktim.Elapsed, AddressOf chkcli
            Print("[INIT]Listen을 시작합니다")
            ServerSoc.SetListen()
            'clichecktim.Start()
            Print("[INIT]소켓 시작 작업 완료")
            Dim fileopener = Hamster_Engine_Project.Project.Hamfile.CopyMe()
            fileopener.SetFile(Application.StartupPath & "\data\allowDispAuthCode.txt", Encoding.UTF8)
            DispAuthCodelist = Split(fileopener.ReadText(), vbCrLf)
            Print("[INIT]엑세스 코드 리스트 읽기 완료")
            Print("[INIT]썸네일 검사 시작")
            MakeThumbNail()
            Print("[INIT]썸네일 검사 완료")
        Catch ex As Exception
            Print("[ERROR]소켓을 초기화하는중 오류가 발생했습니다!!")
            Print(ex.ToString)
        End Try
    End Sub

    Public Sub SocCallback(kind As String, args As Object())
        If kind = "ACCEPT" Then
            Dim clisoc As Socket = args(0)
            Print("[ACCEPT]" & clisoc.RemoteEndPoint.ToString & " -> " & args(1))
        ElseIf kind = "RECEIVE" Then
            ProcessMsg(args(2), args(1))
        ElseIf kind = "SEND" Then
            'Print("[SEND] " & args(1) & "bytes -> ")
            totalSentByte += Convert.ToUInt32(args(1))
            txtSentBytes.Text = "총 전송 : " & totalSentByte.ToString & "byte"
        ElseIf kind = "DISCONNECT" Then
            Print("[DISCONN] " & args(0))
        ElseIf kind = "ERROR" Then
            If (CType(args(1), Exception).GetType.FullName = "System.Net.Sockets.SocketException") Or args(0) = "CFUNC_SEND" Then
                If Thread.CurrentThread.Name.Contains("SEND") Then
                    Print("[ERROR]SENDIMG or SENDTHIMG Abort")
                    Thread.CurrentThread.Abort()
                End If
                Project.LogWrite.DynamicInvoke("[ERROR] (" & args(2).ToString() & ")" & args(0) & " : " & args(1).ToString)
            Else
                Try
                    ServerSoc.CloseClient(args(2).ToString())
                Catch ex As Exception
                    Project.LogWrite.DynamicInvoke("[ERROR]Client Socket Close Fail")
                End Try
                Print("[ERROR] (" & args(2).ToString() & ")" & args(0) & " : " & args(1).ToString)
            End If
        Else
            Print("[" + kind + "]")
        End If
    End Sub

    Private Sub ProcessMsg(data As Byte(), socnum As Integer)
        Try
            Dim httpmsg = Encoding.UTF8.GetString(data)

            If New Regex("^GET").IsMatch(httpmsg) Then 'GET REQ시	
                Dim response As [Byte]() = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine + "Connection: Upgrade" + Environment.NewLine + "Upgrade: websocket" + Environment.NewLine + "Sec-WebSocket-Accept: " + Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(New Regex("Sec-WebSocket-Key: (.*)").Match(httpmsg).Groups(1).Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"))) + Environment.NewLine + Environment.NewLine)
                ServerSoc.Send(response, socnum)
                Print("[SOCKET]" & socnum & "번 소켓에서 웹소켓 핸드셰이크")
            Else 'NON-GET REQ시
                Dim msg As String = DecodeMessage(data)
                'Print("[SOCKET]" & socnum & "번 소켓에서 데이터 수신 : '" & msg & "'")
                Dim pmsg As String() = Split(msg, "#")
                Dim reqName As String = pmsg(0)
                Dim reqdata As JObject
                Try
                    reqdata = JObject.Parse(pmsg(1))
                Catch
                    Print("[ERROR] API Request Data Json Parse Error!")
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
                        End If
                    Else
                        SendREQ("GETCREDENTIAL", New JObject From {{"isNew", True}, {"error", ""}}, socnum)
                    End If
                ElseIf reqName = "MAKESESSION" Then
                    Dim sid As Guid = Guid.NewGuid
                    Dim nowjson As New JObject From {{"sid", sid.ToString}}
                    SendREQ("ISSUESESSION", nowjson, socnum)
                    Dim nowsess = New Session
                    nowsess.SocketNumber = socnum
                    nowsess.SessionStatus = Session.SessionFlag.NoCredential
                    nowsess.CredentialStartTime = Now
                    SessionList.Add(sid.ToString, nowsess)
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
                                    Print("[DISPAUTH] '" & SessionList(nowsid).CredentialUserName & "'(" & SessionList(nowsid).otherData("STUID") & ") -> 코드 '" & reqdata("code").ToString & "' sid : " & nowsid.ToString)
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
                        Dim fileopener As Object = Project.Hamfile.CopyMe()
                        Print("[GETIMGLIST]" & SessionList(nowsid).CredentialUserName & "->" & Application.StartupPath & "\image" & nowpath + "imglist.lst")
                        fileopener.SetFile(Application.StartupPath & "\image" & nowpath + "imglist.lst", Encoding.UTF8)
                        If fileopener.Exist() Then
                            Dim nowlist = Split(fileopener.ReadText(), vbCrLf)
                            'ALBUM,TEST1,테스트 앨범 제목 1,테스트 앨범 제목 2,NONE 썸네일,ALL 권한
                            Dim listtojson As New JArray
                            For Each nowele As String In nowlist
                                Dim nowprop As String() = Split(nowele, ",")
                                If nowprop(0) = "AUTOPHOTO" Then
                                    Dim imgfileobj As Object = Project.Hamfile.CopyMe()
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
            Print(ex.ToString)
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
        Dim fileopener As Object = Project.Hamfile.CopyMe()
        Print("[GETIMG](" & Thread.CurrentThread.GetHashCode & ")" & SessionList(nowsid).CredentialUserName & "->" & Application.StartupPath & "\image" & imgdir)
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
            Dim fileopener As Object = Project.Hamfile.CopyMe()
            'Print("[GETTHUMB](" & Thread.CurrentThread.GetHashCode & ")" & SessionList(nowsid).CredentialUserName & "-> THID : " & thid)
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
            Print(ex.ToString)
            Try
                SendREQ("SHOWALERT", "INTERNAL SERVER ERROR!", socnum)
            Catch
            End Try
        End Try
    End Sub

    Private Class FileEntry
        Public isDirectory As Boolean
        Public Path As String

        Public Sub New(d As Boolean, p As String)
            isDirectory = d
            Path = p
        End Sub
    End Class

    Private imgext As New List(Of String) From {"gif", "bmp", "jpg", "png"}
    Private Sub MakeThumbNail()
        Try
            Dim fileobj As Object = Project.Hamfile.CopyMe()
            Dim entrystack As New Stack
            fileobj.Directory.SetDirectory(Application.StartupPath & "\image\")
            ExploreDirectory(entrystack, Application.StartupPath & "\image\")

            Do Until entrystack.Count = 0
                Try
                    Dim nowe As FileEntry = entrystack.Pop

                    If nowe.isDirectory Then
                        ExploreDirectory(entrystack, nowe.Path)
                    Else
                        Dim nowehash = ComputeFileHash(nowe.Path)
                        Dim thpath = Path.ChangeExtension(Application.StartupPath & "\thumb\" & nowehash, GetFileType(nowe.Path))
                        Dim filechkobj As Object = Project.Hamfile.CopyMe()
                        filechkobj.SetFile(thpath, Encoding.UTF8)
                        If Not filechkobj.Exist() Then
                            If isImageFilePath(nowe.Path) Then
                                Print("[THUMB] '" & nowe.Path & "' -> (" & thpath & ")의 썸네일 생성 시작")
                                Dim img As Image = Image.FromFile(nowe.Path)
                                Dim th As Image = img.GetThumbnailImage(480, 320, Function() False, IntPtr.Zero)
                                th.Save(thpath)
                                th.Dispose()
                                img.Dispose()
                                Print("[THUMB] 썸네일 생성 완료")
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Print("[THUMB] 개별 사진 처리중 오류 발생 " & ex.ToString)
                Finally
                    Application.DoEvents()
                End Try
            Loop
        Catch ex As Exception
            Print("[THUMB] 초기화 중 오류 발생 " & ex.ToString)
        End Try
    End Sub

    Private Function GetFileType(Path As String) As String
        Return Path.Split(".").Last
    End Function

    Private Function isImageFilePath(Path As String) As Boolean
        Return imgext.Contains(GetFileType(Path).ToLower)
    End Function

    Private Function ComputeFileHash(path As String) As String
        Using hashobj As MD5 = MD5.Create
            Using stream As FileStream = File.OpenRead(path)
                Return BitConverter.ToString(hashobj.ComputeHash(stream)).Replace("-", "").ToLowerInvariant()
            End Using
        End Using
    End Function

    Private Sub ExploreDirectory(ByRef stack As Stack, path As String)
        Dim fileobj As Object = Project.Hamfile.CopyMe()
        fileobj.Directory.SetDirectory(path)
        Dim entry As String() = fileobj.Directory.GetFile()
        For Each e In entry
            Dim nowe = New FileEntry(False, e)
            stack.Push(nowe)
        Next
        entry = fileobj.Directory.GetDirectory()
        For Each e In entry
            Dim nowe = New FileEntry(True, e)
            stack.Push(nowe)
        Next
    End Sub

    Public Sub Print(data As String)
        Try
            lstLog.Items.Add(data)
            lstLog.SelectedIndex = lstLog.Items.Count - 1
            Project.LogWrite.DynamicInvoke(data)
        Catch
        End Try
    End Sub

    Private Sub lstLog_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstLog.SelectedIndexChanged

    End Sub

    Public Function EncodeMessage(data As String) As Byte()
        Dim lb = New List(Of Byte)()
        lb.Add(&H81)
        Dim bytedata As Byte() = Encoding.UTF8.GetBytes(data)
        Dim size = bytedata.Length

        If size > 127 Then
            'Throw New Exception("단일프레임내 127바이트 이상의 데이터가 포함될수 없습니다. (Data must be less than 128 bytes)")
        End If

        lb.Add(CByte(size))
        lb.AddRange(bytedata)
        Return lb.ToArray
    End Function

    Private Enum messageType
        Continuos
        Text
        Binary
        Close
        Ping
        Pong
    End Enum

    Private Function CreateFrame(message As String, Optional messageType As messageType = messageType.Text, Optional messageContinues As Boolean = False) As Byte()
        Dim b1 As Byte = 0
        Dim b2 As Byte = 0
        Select Case messageType
            Case messageType.Continuos
                b1 = 0
            Case messageType.Text
                b1 = 1
            Case messageType.Binary
                b1 = 2
            Case messageType.Close
                b1 = 8
            Case messageType.Ping
                b1 = 9
            Case messageType.Pong
                b1 = 10
        End Select

        b1 = CByte((b1 + 128))
        Dim messageBytes As Byte() = Encoding.UTF8.GetBytes(message)
        If messageBytes.Length < 126 Then
            b2 = CByte(messageBytes.Length)
        Else
            If messageBytes.Length < Math.Pow(2, 16) - 1 Then
                b2 = 126
            Else
                b2 = 127
            End If
        End If

        Dim frame As Byte() = Nothing
        If b2 < 126 Then
            frame = New Byte(messageBytes.Length + 2 - 1) {}
            frame(0) = b1
            frame(1) = b2
            Array.Copy(messageBytes, 0, frame, 2, messageBytes.Length)
        End If

        If b2 = 126 Then
            frame = New Byte(messageBytes.Length + 4 - 1) {}
            frame(0) = b1
            frame(1) = b2
            Dim lenght As Byte() = BitConverter.GetBytes(messageBytes.Length)
            frame(2) = lenght(1)
            frame(3) = lenght(0)
            Array.Copy(messageBytes, 0, frame, 4, messageBytes.Length)
        End If

        If b2 = 127 Then
            frame = New Byte(messageBytes.Length + 10 - 1) {}
            frame(0) = b1
            frame(1) = b2
            Dim lenght As Byte() = BitConverter.GetBytes(CLng(messageBytes.Length))

            Dim i = 7, j = 2
            While i >= 0
                If Not i >= 0 Then
                    Exit While
                End If
                frame(j) = lenght(i)
                i -= 1
                j += 1
            End While
        End If

        Return frame
    End Function

    Public Function DecodeMessage(buffer As Byte()) As String
        Dim b As Byte = buffer(1)
        Dim dataLength As Integer = 0
        Dim totalLength As Integer = 0
        Dim keyIndex As Integer = 0

        If b - 128 <= 125 Then
            dataLength = b - 128
            keyIndex = 2
            totalLength = dataLength + 6
        End If

        If b - 128 = 126 Then
            dataLength = BitConverter.ToInt16(New Byte() {buffer(3), buffer(2)}, 0)
            keyIndex = 4
            totalLength = dataLength + 8
        End If

        If b - 128 = 127 Then
            dataLength = CInt(BitConverter.ToInt64(New Byte() {buffer(9), buffer(8), buffer(7), buffer(6), buffer(5), buffer(4),
                buffer(3), buffer(2)}, 0))
            keyIndex = 10
            totalLength = dataLength + 14
        End If

        Dim key As Byte() = New Byte() {buffer(keyIndex), buffer(keyIndex + 1), buffer(keyIndex + 2), buffer(keyIndex + 3)}

        Dim dataIndex As Integer = keyIndex + 4
        Dim count As Integer = 0
        For i As Integer = dataIndex To totalLength - 1
            buffer(i) = CByte(buffer(i) Xor key(count Mod 4))
            count += 1
        Next

        Return Encoding.ASCII.GetString(buffer, dataIndex, dataLength)
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

    Private Sub bthHalt_Click(sender As Object, e As EventArgs) Handles bthHalt.Click
        Try
            ServerSoc.Shutdown()
        Catch
        End Try

        Project.EngineShdw.DynamicInvoke()
    End Sub
End Class
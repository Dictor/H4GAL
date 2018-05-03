Imports System.Net.Sockets
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports Newtonsoft.Json.Linq

Public Class frmMain
    Private Sub frmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Me.Shown, AddressOf initproc
    End Sub

    Private ServerSoc As Object
    Delegate Sub SocCb(kind As String, args As Object())
    Private clichecktim As New System.Timers.Timer(100)
    Private questionlist As String()
    Dim questionSelector As Random = New Random()

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

    Public Sub initproc()
        Print("[INIT]UI 로드 완료")
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
            fileopener.SetFile(Application.StartupPath & "\data\question.txt", Encoding.UTF8)
            questionlist = Split(fileopener.ReadText(), vbCrLf)
            Print("[INIT]질문 리스트 읽기 완료")
        Catch ex As Exception
            Print("[ERROR]소켓을 초기화하는중 오류가 발생했습니다!!")
            Print(ex.ToString)
        End Try
    End Sub

    Public Sub SocCallback(kind As String, args As Object())
        If kind = "ACCEPT" Then
            Dim clisoc As Socket = args(0)
            Print("[ACCEPT]" & clisoc.RemoteEndPoint.ToString & " " & args(1))
        ElseIf kind = "RECEIVE" Then
            ProcessMsg(args(2), args(1))
        ElseIf kind = "SEND" Then
            Print("[SEND] " & args(1))
        ElseIf kind = "DISCONNECT" Then
            Print("[DISCONN] " & args(0))
        ElseIf kind = "ERROR" Then
            Print("[ERROR] " & args(0) & " : " & args(1).ToString)
        Else
            Print("[" + kind + "]")
        End If
    End Sub

    Private Sub chkcli()
        ServerSoc.SendAll(EncodeMessage("dummy_checkconn"))
        ServerSoc.pCheckClientConnect()
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
                Print("[SOCKET]" & socnum & "번 소켓에서 데이터 수신 : '" & msg & "'")
                Dim pmsg As String() = Split(msg, "#")
                If pmsg(0) = "SESSION" Then
                    Dim nowsid = JObject.Parse(pmsg(1))("sid")
                    If SessionList.ContainsKey(nowsid) Then
                        If SessionList(nowsid).SessionStatus = Session.SessionFlag.NoCredential Then
                            SendREQ("GETCREDENTIAL", New JObject From {{"isNew", False}, {"status", "empty"}}, socnum)
                        ElseIf SessionList(nowsid).SessionStatus = Session.SessionFlag.DisposableCredential Then
                            SendREQ("GETCREDENTIAL", New JObject From {{"isNew", False}, {"status", "disposable"}}, socnum)
                        ElseIf SessionList(nowsid).SessionStatus = Session.SessionFlag.AccountCredential Then
                            SendREQ("GETCREDENTIAL", New JObject From {{"isNew", False}, {"status", "account"}}, socnum)
                        End If
                    Else
                        SendREQ("GETCREDENTIAL", New JObject From {{"isNew", True}, {"error", ""}}, socnum)
                    End If
                ElseIf pmsg(0) = "MAKESESSION" Then
                    Dim sid As Guid = Guid.NewGuid
                    Dim nowjson As New JObject From {{"sid", sid.ToString}}
                    SendREQ("ISSUESESSION", nowjson, socnum)
                    Dim nowsess = New Session
                    nowsess.SocketNumber = socnum
                    nowsess.SessionStatus = Session.SessionFlag.NoCredential
                    nowsess.CredentialStartTime = Now
                    SessionList.Add(sid.ToString, nowsess)
                ElseIf pmsg(0) = "TRYDISPAUTH" Then
                    Dim nowsid = JObject.Parse(pmsg(1))("sid")

                ElseIf pmsg(0) = "GETDISPQUESTION" Then
                    Dim nowsid = JObject.Parse(pmsg(1))("sid")
                    Dim nowquestion As String = questionlist(questionSelector.Next(0, questionlist.Length))
                    If SessionList(nowsid).otherData.ContainsKey("QANSWER") Then
                        SendREQ("GETDISPQUESTION", New JObject From {{"status", True}, {"question", SessionList(nowsid).otherData("QQUESTION")}}, socnum)
                    Else
                        Dim escapequestion = Uri.EscapeUriString(Split(nowquestion, ",")(0))
                        SessionList(nowsid).otherData.Add("QQUESTION", Split(nowquestion, ",")(0))
                        SessionList(nowsid).otherData.Add("QANSWER", Split(nowquestion, ",")(1))
                        SendREQ("GETDISPQUESTION", New JObject From {{"status", True}, {"question", escapequestion}}, socnum)
                    End If
                ElseIf pmsg(0) = "TRYANSWER" Then
                    Dim nowsid = JObject.Parse(pmsg(1))("sid")
                    Dim clians = Uri.UnescapeDataString(JObject.Parse(pmsg(1))("answer"))
                    If SessionList(nowsid).otherData.ContainsKey("QANSWER") Then
                        If clians = SessionList(nowsid).otherData("QANSWER") Then
                            SessionList(nowsid).SessionStatus = Session.SessionFlag.DisposableCredential
                            SendREQ("TRYANSWER", New JObject From {{"status", True}}, socnum)
                        Else
                            SendREQ("TRYANSWER", New JObject From {{"status", False}}, socnum)
                        End If
                    Else
                        SendREQ("TRYANSWER", New JObject From {{"status", False}}, socnum)
                    End If
                Else
                End If
            End If
        Catch ex As Exception
            Print(ex.ToString)
        End Try
    End Sub

    Public Sub SendREQ(APIname As String, APIdata As JObject, socnum As Integer)
        ServerSoc.Send(CreateFrame(APIname + "#" + APIdata.ToString), socnum)
    End Sub

    Public Sub Print(data As String)
        lstLog.Items.Add(data)
        lstLog.SelectedIndex = lstLog.Items.Count - 1
        Try
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
End Class
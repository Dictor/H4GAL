Imports Hamster_Engine_Project.HE_Common_Component

Public Class Project
    Public Shared Version As New HamsterVersion("Hamster Engine Default", 0, 0, 0, 0)

    '("개체 식별자 문자열", {"어셈블리 경로"})
    Private Shared LoadObject_PROJ_Info As New Dictionary(Of String, Object())

    Private Shared ApplicationStartupPath As String

    Public Function initialization(EngineAsm As Dictionary(Of String, Object), enginefunc As [Delegate]()(), args As Object()) As Dictionary(Of String, Object())
        MsgBox("init")
        ApplicationStartupPath = args(0)

        Return LoadObject_PROJ_Info
    End Function

    Public Sub main(ProjsideAsm As Dictionary(Of String, Object))
        MsgBox("main")
    End Sub
End Class

Imports System.Drawing
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Windows.Forms

Module Thumbnail
    Private ReadOnly thApplyFileExt As New List(Of String) From {"gif", "bmp", "jpg", "png"}

    Public Sub MakeThumbNail()
        Try
            Dim fileobj As Object = Project.HEfile.CopyMe()
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
                        Dim filechkobj As Object = Project.HEfile.CopyMe()
                        filechkobj.SetFile(thpath, Encoding.UTF8)
                        If Not filechkobj.Exist() Then
                            If isImageFilePath(nowe.Path) Then
                                Print("THUMB", "", "'" & nowe.Path & "' -> (" & thpath & ")의 썸네일 생성 시작")
                                Dim img As Image = Image.FromFile(nowe.Path)
                                Dim th As Image = img.GetThumbnailImage(480, 320, Function() False, IntPtr.Zero)
                                th.Save(thpath)
                                th.Dispose()
                                img.Dispose()
                                Print("THUMB", "", "썸네일 생성 완료")
                            End If
                        End If
                    End If
                Catch ex As Exception
                    Print("ERROR", "", "썸네일 개별 사진 처리중 오류 발생 " & ex.ToString)
                Finally
                    Application.DoEvents()
                End Try
            Loop
        Catch ex As Exception
            Print("ERROR", "", " 썸네일 초기화 중 오류 발생 " & ex.ToString)
        End Try
    End Sub

    Private Function GetFileType(Path As String) As String
        Return Path.Split(".").Last
    End Function

    Public Function isImageFilePath(Path As String) As Boolean
        Return thApplyFileExt.Contains(GetFileType(Path).ToLower)
    End Function

    Public Function ComputeFileHash(path As String) As String
        Using hashobj As MD5 = MD5.Create
            Using stream As FileStream = File.OpenRead(path)
                Return BitConverter.ToString(hashobj.ComputeHash(stream)).Replace("-", "").ToLowerInvariant()
            End Using
        End Using
    End Function

    Public Sub ExploreDirectory(ByRef stack As Stack, path As String)
        Dim fileobj As Object = Project.HEfile.CopyMe()
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

    Private Class FileEntry
        Public isDirectory As Boolean
        Public Path As String

        Public Sub New(d As Boolean, p As String)
            isDirectory = d
            Path = p
        End Sub
    End Class
End Module

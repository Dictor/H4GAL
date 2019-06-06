<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form은 Dispose를 재정의하여 구성 요소 목록을 정리합니다.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows Form 디자이너에 필요합니다.
    Private components As System.ComponentModel.IContainer

    '참고: 다음 프로시저는 Windows Form 디자이너에 필요합니다.
    '수정하려면 Windows Form 디자이너를 사용하십시오.  
    '코드 편집기에서는 수정하지 마세요.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.txtSentBytes = New System.Windows.Forms.Label()
        Me.bthHalt = New System.Windows.Forms.Button()
        Me.txtTotalAccept = New System.Windows.Forms.Label()
        Me.txtTotalHandshake = New System.Windows.Forms.Label()
        Me.lstLog = New System.Windows.Forms.ListView()
        Me.colTime = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colKind = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colUser = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colContent = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.txtAvailSess = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtSentBytes
        '
        Me.txtSentBytes.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtSentBytes.AutoSize = True
        Me.txtSentBytes.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtSentBytes.Location = New System.Drawing.Point(12, 395)
        Me.txtSentBytes.Name = "txtSentBytes"
        Me.txtSentBytes.Size = New System.Drawing.Size(70, 19)
        Me.txtSentBytes.TabIndex = 1
        Me.txtSentBytes.Text = "총 전송 : "
        '
        'bthHalt
        '
        Me.bthHalt.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.bthHalt.Font = New System.Drawing.Font("맑은 고딕", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.bthHalt.ForeColor = System.Drawing.Color.Red
        Me.bthHalt.Location = New System.Drawing.Point(741, 390)
        Me.bthHalt.Name = "bthHalt"
        Me.bthHalt.Size = New System.Drawing.Size(75, 23)
        Me.bthHalt.TabIndex = 2
        Me.bthHalt.Text = "종료"
        Me.bthHalt.UseVisualStyleBackColor = True
        '
        'txtTotalAccept
        '
        Me.txtTotalAccept.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.txtTotalAccept.AutoSize = True
        Me.txtTotalAccept.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtTotalAccept.Location = New System.Drawing.Point(178, 395)
        Me.txtTotalAccept.Name = "txtTotalAccept"
        Me.txtTotalAccept.Size = New System.Drawing.Size(121, 19)
        Me.txtTotalAccept.TabIndex = 3
        Me.txtTotalAccept.Text = "총 소켓 Accept : "
        '
        'txtTotalHandshake
        '
        Me.txtTotalHandshake.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.txtTotalHandshake.AutoSize = True
        Me.txtTotalHandshake.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtTotalHandshake.Location = New System.Drawing.Point(381, 395)
        Me.txtTotalHandshake.Name = "txtTotalHandshake"
        Me.txtTotalHandshake.Size = New System.Drawing.Size(89, 19)
        Me.txtTotalHandshake.TabIndex = 4
        Me.txtTotalHandshake.Text = "총 WS H/S: "
        '
        'lstLog
        '
        Me.lstLog.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstLog.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colTime, Me.colKind, Me.colUser, Me.colContent})
        Me.lstLog.Location = New System.Drawing.Point(12, 12)
        Me.lstLog.Name = "lstLog"
        Me.lstLog.Size = New System.Drawing.Size(804, 373)
        Me.lstLog.TabIndex = 5
        Me.lstLog.UseCompatibleStateImageBehavior = False
        Me.lstLog.View = System.Windows.Forms.View.Details
        '
        'colTime
        '
        Me.colTime.Text = "시간"
        Me.colTime.Width = 100
        '
        'colKind
        '
        Me.colKind.Text = "종류"
        Me.colKind.Width = 100
        '
        'colUser
        '
        Me.colUser.Text = "세션"
        Me.colUser.Width = 80
        '
        'colContent
        '
        Me.colContent.Text = "내용"
        '
        'txtAvailSess
        '
        Me.txtAvailSess.Anchor = System.Windows.Forms.AnchorStyles.Bottom
        Me.txtAvailSess.AutoSize = True
        Me.txtAvailSess.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtAvailSess.Location = New System.Drawing.Point(547, 395)
        Me.txtAvailSess.Name = "txtAvailSess"
        Me.txtAvailSess.Size = New System.Drawing.Size(98, 19)
        Me.txtAvailSess.TabIndex = 6
        Me.txtAvailSess.Text = "총 활성 세션: "
        '
        'frmMain
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(828, 422)
        Me.Controls.Add(Me.txtAvailSess)
        Me.Controls.Add(Me.lstLog)
        Me.Controls.Add(Me.txtTotalHandshake)
        Me.Controls.Add(Me.txtTotalAccept)
        Me.Controls.Add(Me.bthHalt)
        Me.Controls.Add(Me.txtSentBytes)
        Me.MinimumSize = New System.Drawing.Size(840, 453)
        Me.Name = "frmMain"
        Me.Text = "frmMain"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtSentBytes As System.Windows.Forms.Label
    Friend WithEvents bthHalt As System.Windows.Forms.Button
    Friend WithEvents txtTotalAccept As System.Windows.Forms.Label
    Friend WithEvents txtTotalHandshake As System.Windows.Forms.Label
    Friend WithEvents lstLog As System.Windows.Forms.ListView
    Friend WithEvents colTime As System.Windows.Forms.ColumnHeader
    Friend WithEvents colKind As System.Windows.Forms.ColumnHeader
    Friend WithEvents colUser As System.Windows.Forms.ColumnHeader
    Friend WithEvents colContent As System.Windows.Forms.ColumnHeader
    Friend WithEvents txtAvailSess As System.Windows.Forms.Label
End Class

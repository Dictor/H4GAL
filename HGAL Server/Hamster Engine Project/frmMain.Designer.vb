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
        Me.lstLog = New System.Windows.Forms.ListBox()
        Me.txtSentBytes = New System.Windows.Forms.Label()
        Me.bthHalt = New System.Windows.Forms.Button()
        Me.txtTotalAccept = New System.Windows.Forms.Label()
        Me.txtTotalHandshake = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lstLog
        '
        Me.lstLog.Font = New System.Drawing.Font("맑은 고딕", 8.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.lstLog.FormattingEnabled = True
        Me.lstLog.HorizontalExtent = 3000
        Me.lstLog.HorizontalScrollbar = True
        Me.lstLog.Location = New System.Drawing.Point(13, 13)
        Me.lstLog.Name = "lstLog"
        Me.lstLog.Size = New System.Drawing.Size(793, 355)
        Me.lstLog.TabIndex = 0
        '
        'txtSentBytes
        '
        Me.txtSentBytes.AutoSize = True
        Me.txtSentBytes.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtSentBytes.Location = New System.Drawing.Point(12, 387)
        Me.txtSentBytes.Name = "txtSentBytes"
        Me.txtSentBytes.Size = New System.Drawing.Size(70, 19)
        Me.txtSentBytes.TabIndex = 1
        Me.txtSentBytes.Text = "총 전송 : "
        '
        'bthHalt
        '
        Me.bthHalt.Font = New System.Drawing.Font("맑은 고딕", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.bthHalt.ForeColor = System.Drawing.Color.Red
        Me.bthHalt.Location = New System.Drawing.Point(731, 383)
        Me.bthHalt.Name = "bthHalt"
        Me.bthHalt.Size = New System.Drawing.Size(75, 23)
        Me.bthHalt.TabIndex = 2
        Me.bthHalt.Text = "종료"
        Me.bthHalt.UseVisualStyleBackColor = True
        '
        'txtTotalAccept
        '
        Me.txtTotalAccept.AutoSize = True
        Me.txtTotalAccept.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtTotalAccept.Location = New System.Drawing.Point(226, 387)
        Me.txtTotalAccept.Name = "txtTotalAccept"
        Me.txtTotalAccept.Size = New System.Drawing.Size(121, 19)
        Me.txtTotalAccept.TabIndex = 3
        Me.txtTotalAccept.Text = "총 소켓 Accept : "
        '
        'txtTotalHandshake
        '
        Me.txtTotalHandshake.AutoSize = True
        Me.txtTotalHandshake.Font = New System.Drawing.Font("맑은 고딕", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(129, Byte))
        Me.txtTotalHandshake.Location = New System.Drawing.Point(445, 386)
        Me.txtTotalHandshake.Name = "txtTotalHandshake"
        Me.txtTotalHandshake.Size = New System.Drawing.Size(89, 19)
        Me.txtTotalHandshake.TabIndex = 4
        Me.txtTotalHandshake.Text = "총 WS H/S: "
        '
        'frmMain
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(824, 448)
        Me.Controls.Add(Me.txtTotalHandshake)
        Me.Controls.Add(Me.txtTotalAccept)
        Me.Controls.Add(Me.bthHalt)
        Me.Controls.Add(Me.txtSentBytes)
        Me.Controls.Add(Me.lstLog)
        Me.MaximumSize = New System.Drawing.Size(840, 487)
        Me.MinimumSize = New System.Drawing.Size(840, 487)
        Me.Name = "frmMain"
        Me.Text = "frmMain"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents lstLog As System.Windows.Forms.ListBox
    Friend WithEvents txtSentBytes As System.Windows.Forms.Label
    Friend WithEvents bthHalt As System.Windows.Forms.Button
    Friend WithEvents txtTotalAccept As System.Windows.Forms.Label
    Friend WithEvents txtTotalHandshake As System.Windows.Forms.Label
End Class

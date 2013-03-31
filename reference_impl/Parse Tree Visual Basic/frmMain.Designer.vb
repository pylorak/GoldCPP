<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.txtSource = New System.Windows.Forms.TextBox
        Me.txtParseTree = New System.Windows.Forms.TextBox
        Me.btnParse = New System.Windows.Forms.Button
        Me.txtTableFile = New System.Windows.Forms.TextBox
        Me.btnLoad = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'txtSource
        '
        Me.txtSource.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtSource.Location = New System.Drawing.Point(8, 56)
        Me.txtSource.Multiline = True
        Me.txtSource.Name = "txtSource"
        Me.txtSource.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtSource.Size = New System.Drawing.Size(668, 140)
        Me.txtSource.TabIndex = 0
        Me.txtSource.Text = "Display ""hello world"""
        '
        'txtParseTree
        '
        Me.txtParseTree.BackColor = System.Drawing.SystemColors.Window
        Me.txtParseTree.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtParseTree.Location = New System.Drawing.Point(8, 256)
        Me.txtParseTree.Multiline = True
        Me.txtParseTree.Name = "txtParseTree"
        Me.txtParseTree.ReadOnly = True
        Me.txtParseTree.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtParseTree.Size = New System.Drawing.Size(668, 148)
        Me.txtParseTree.TabIndex = 1
        '
        'btnParse
        '
        Me.btnParse.Location = New System.Drawing.Point(580, 204)
        Me.btnParse.Name = "btnParse"
        Me.btnParse.Size = New System.Drawing.Size(96, 32)
        Me.btnParse.TabIndex = 2
        Me.btnParse.Text = "Parse"
        Me.btnParse.UseVisualStyleBackColor = True
        '
        'txtTableFile
        '
        Me.txtTableFile.Font = New System.Drawing.Font("Courier New", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtTableFile.Location = New System.Drawing.Point(8, 12)
        Me.txtTableFile.Name = "txtTableFile"
        Me.txtTableFile.Size = New System.Drawing.Size(564, 21)
        Me.txtTableFile.TabIndex = 3
        '
        'btnLoad
        '
        Me.btnLoad.Location = New System.Drawing.Point(580, 12)
        Me.btnLoad.Name = "btnLoad"
        Me.btnLoad.Size = New System.Drawing.Size(96, 32)
        Me.btnLoad.TabIndex = 4
        Me.btnLoad.Text = "Load"
        Me.btnLoad.UseVisualStyleBackColor = True
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(684, 412)
        Me.Controls.Add(Me.btnLoad)
        Me.Controls.Add(Me.txtTableFile)
        Me.Controls.Add(Me.btnParse)
        Me.Controls.Add(Me.txtParseTree)
        Me.Controls.Add(Me.txtSource)
        Me.Name = "frmMain"
        Me.Text = "Draw Parse Tree"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtSource As System.Windows.Forms.TextBox
    Friend WithEvents txtParseTree As System.Windows.Forms.TextBox
    Friend WithEvents btnParse As System.Windows.Forms.Button
    Friend WithEvents txtTableFile As System.Windows.Forms.TextBox
    Friend WithEvents btnLoad As System.Windows.Forms.Button

End Class

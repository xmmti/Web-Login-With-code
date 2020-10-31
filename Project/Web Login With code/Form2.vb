Public Class Form2
    Public URL_path As String
    Public Cookie_ As String
    Dim _choice As String
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Not RadioButton1.Checked AndAlso Not RadioButton2.Checked Then
            MsgBox("Select One!")
        Else
            If Form1.IG_Send_Code(_choice, Cookie_, URL_path) Then
                Me.Text &= " - Wait To Enter Code"
                Button1.Enabled = False
            Else
                MsgBox("Someting is worng!", MsgBoxStyle.Critical)
            End If
        End If

    End Sub
    Private Sub RadioButton1_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton1.CheckedChanged
        _choice = RadioButton1.Tag
    End Sub

    Private Sub RadioButton2_CheckedChanged(sender As Object, e As EventArgs) Handles RadioButton2.CheckedChanged
        _choice = RadioButton2.Tag
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If Form1.IG_Enter_Code(TextBox1.Text, Cookie_, URL_path) Then
            MsgBox("logged !!")
            Form1.Label3.Text = "logged"
            Me.Hide()
        Else
            MsgBox("Someting is worng!", MsgBoxStyle.Critical)
        End If
        Button2.Enabled = True
    End Sub
End Class
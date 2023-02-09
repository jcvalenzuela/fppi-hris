Imports System.Data.SqlClient
Module GlobalFunction
    Public Sub DTTimeFormat(ByVal timeCaption As String, ByVal lblTime As Label)
        Dim hourformat As Integer
        If Now.Hour > 12 Then
            hourformat = Now.Hour
            lblTime.Text = timeCaption + hourformat.ToString("00") + ":" + Now.Minute.ToString("00") + ":" + Now.Second.ToString("00") + " PM"
        ElseIf Now.Hour = 12 Then
            hourformat = 12
            lblTime.Text = timeCaption + hourformat.ToString("00") + ":" + Now.Minute.ToString("00") + ":" + Now.Second.ToString("00") + " PM"
        ElseIf Now.Hour = 0 Then
            hourformat = Now.Hour
            lblTime.Text = timeCaption + hourformat.ToString("00") + ":" + Now.Minute.ToString("00") + ":" + Now.Second.ToString("00") + " AM"
        Else
            lblTime.Text = timeCaption + Now.Hour.ToString("00") + ":" + Now.Minute.ToString("00") + ":" + Now.Second.ToString("00") + " AM"
        End If
    End Sub
End Module

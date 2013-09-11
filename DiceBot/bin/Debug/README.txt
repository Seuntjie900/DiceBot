after this. 

-add a referenze to the Skybound.Gecko.dll

-add a new class (called whatever you like (fastwebcontrol.vb))

///
Imports Skybound.Gecko
Public Class fastwebcontrol
    Inherits GeckoWebBrowser
End Class
///


project example

///
Imports Skybound.Gecko

Public Class Form1
    Sub New()
        InitializeComponent()
        Xpcom.Initialize(Environment.CurrentDirectory + "/xulrunner")

    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Fasterbrowser1.Navigate("https://just-dice.com/")
    End Sub
End Class
///



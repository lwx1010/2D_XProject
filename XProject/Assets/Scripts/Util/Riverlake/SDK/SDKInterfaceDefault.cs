using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public sealed class SDKInterfaceDefault : SDKInterface
{
    public override void Init()
    {
        throw new NotImplementedException();
    }

    public override void Login()
    {

    }

    /*public override void LoginCustom(string customData)
    {
        throw new NotImplementedException();
    }*/

    /*public override void SwitchLogin()
    {
        throw new NotImplementedException();
    }*/

    public override bool Logout()
    {
        throw new NotImplementedException();
    }

    public override bool ShowAccountCenter()
    {
        throw new NotImplementedException();
    }

    public override void SubmitGameData(ExtraGameData data)
    {
        throw new NotImplementedException();
    }

    public override bool SDKExit()
    {
        throw new NotImplementedException();
    }

    /*public override void Pay(PayParams data)
    {
        throw new NotImplementedException();
    }*/

    public override void OrderAndPay(PayParams data)
    {
        throw new NotImplementedException();
    }

    public override bool IsSupportExit()
    {
        throw new NotImplementedException();
    }

    public override bool IsSupportAccountCenter()
    {
        throw new NotImplementedException();
    }

    public override bool IsSupportLogout()
    {
        throw new NotImplementedException();
    }

    public override string GetMacAddr()
    {
        throw new NotImplementedException();
    }

    public override string GetIpAddr()
    {
        throw new NotImplementedException();
    }

    public override void GetSDKInfo()
    {
        throw new NotImplementedException();
    }

    public override bool IsIdentify()
    {
        throw new NotImplementedException();
    }

    public override bool IsAudlt()
    {
        throw new NotImplementedException();
    }
}
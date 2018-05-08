using CinemaDirector;
using LuaFramework;
using LuaInterface;
using UnityEngine;


/// <summary>
/// 显示与场景实体相关的图片名称界面
/// </summary>
[CutsceneItemAttribute("GUI", "Texture Name", CutsceneItemGenre.GlobalItem)]
public class ShowTextureNameUIEvent : CinemaGlobalEvent
{
    public enum EDirection
    {
        LeftTop = 0, RightTop , LeftBottom , RightBottom
    }
    
    public string ImageName;

    public Vector2 Size;

    public EDirection Direction;
    
    public override void Trigger()
    {
        if (!Application.isPlaying) return;
        
        LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>();
        
        LuaFunction func = luaMgr.mainLua.GetFunction("EntityTextureNamePanel.show");
        
        func.BeginPCall();
        func.Push(ImageName);
        func.Push(Size);
        func.Push((int)Direction);
        func.PCall();
        func.EndPCall();
    }



    public override void Reverse()
    {
        if (!Application.isPlaying) return;

        PanelManager panelMgr = AppFacade.Instance.GetManager<PanelManager>();
        panelMgr.ClosePanel("EntityTextureName");
    }

}

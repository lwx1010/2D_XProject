using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 剧情Facade启动
/// </summary>
public class PlotFacade : AppFacade
{
    private static PlotFacade mPlotInstance; 
    public PlotFacade() : base()
    {
    }

    private static PlotFacade _instance;

    new public static PlotFacade Instance
    {
        get{
            if (_instance != mPlotInstance || _instance == null) {
                _instance = new PlotFacade();
                mPlotInstance = (PlotFacade)_instance;
            }
            return (PlotFacade)_instance;
        }
    }

    override protected void InitFramework()
    {
        base.InitFramework();
        RegisterCommand("Plot_START_UP", typeof(PlotStartUpCommand));
    }


    public void StartUpPlot()
    {   
        this.SendMessageCommand("Plot_START_UP");
        this.RemoveMultiCommand("Plot_START_UP");
    }

    public void Destroy()
    {
        mPlotInstance = null;
        _instance = null;
    }
}


﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成
//     如果重新生成代码，将丢失对此文件所做的更改。
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GestureModule : VisualFeatureModule
{

    public GestureModule(Classifier classifier) : base(classifier) { }

    public void OnNewFrameDataReady(Object sender, DataTransferEventArgs args)
    {
        Console.WriteLine("New frame callback from gesture current frame:" + args.m_data);
        Console.WriteLine("x="+m_dataWarehouse.GetPlayer1CurrentPosition().X+
                          "  y="+m_dataWarehouse.GetPlayer1CurrentPosition().Y+
                          "  z="+m_dataWarehouse.GetPlayer1CurrentPosition().Z);

    }


}


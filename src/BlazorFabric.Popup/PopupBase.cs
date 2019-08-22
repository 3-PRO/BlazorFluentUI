﻿using BlazorFabric.BaseComponent;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlazorFabric.Popup
{
    /**
     * This adds accessibility to Dialog and Panel controls
     */
    public class PopupBase : FabricComponentBase
    {
        internal PopupBase() { }

        [Parameter] public RenderFragment ChildContent { get; set; }

        [Parameter] public string Role { get; set; }
        [Parameter] public string AriaLabel { get; set; }
        [Parameter] public string AriaLabelledBy { get; set; }
        [Parameter] public string AriaDescribedBy { get; set; }
        [Parameter] public bool ShouldRestoreFocus { get; set; } = true;  //THIS DOES NOTHING AT THE MOMENT.  

        [Parameter] public EventCallback<UIEventArgs> OnDismiss { get; set; }


        // Come back to this later if needed!
        // Line needed on razor page:
        // style=@("overflowY: {(needsVerticalScrollBar ? "scroll" : "hidden")}; outline: none")
        protected bool needsVerticalScrollBar = false;


        protected async Task KeyDownHandler(UIKeyboardEventArgs args)
        {
            switch (args.Key)
            {
                case "Escape":
                    await OnDismiss.InvokeAsync(args);
                    break;
            }

            //return Task.CompletedTask;
        }
    }
}

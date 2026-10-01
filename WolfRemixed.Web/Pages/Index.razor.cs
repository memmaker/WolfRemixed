using System;
using Microsoft.JSInterop;
using Microsoft.Xna.Framework;

namespace WolfRemixed_Web.Pages
{
    public partial class Index
    {
        Game _game;

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                JsRuntime.InvokeAsync<object>("initRenderJS", DotNetObjectReference.Create(this));
            }
        }

        [JSInvokable]
        public void TickDotNet()
        {
            // init game
            if (_game == null)
            {
                var js = (IJSInProcessRuntime)JsRuntime;
                Twengine.SubSystems.Raycast.InputHandler.ReadPointerDeltaX = () => js.Invoke<float>("takeMouseDeltaX");
                _game = new raycaster.RaycastGame();
                _game.Run();
            }

            // run gameloop
            _game.Tick();
        }

    }
}

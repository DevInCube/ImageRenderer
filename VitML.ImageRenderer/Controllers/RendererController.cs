using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Views;

namespace VitML.ImageRenderer.Controllers
{
    public class RendererController
    {

        private Renderer renderer;

        public RendererController(Renderer renderer)
        {
            this.renderer = renderer;
        }

        public void Render(BitmapImage image)
        {
            renderer.Render(image);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VitML.ImageRenderer.Loaders;
using VitML.ImageRenderer.Models;

namespace VitML.ImageRenderer.App.Models
{
    public class WindowConfig : RendererConfig
    {

        public string Title { get; set; }

        public WindowConfig()
        {
            Title = "Renderer";
        }

        public override void OnSerialize(SerializationInfo info)
        {
            base.OnSerialize(info);
            info.Add("title", this.Title);
        }

        public override void OnDeserialize(SerializationInfo info)
        {
            base.OnDeserialize(info);
            try
            {
                this.Title = info.Get("title");
            }
            catch
            {
                this.Title = "Renderer";
            }
        }

        internal void Copy(WindowConfig n)
        {
            this.Compress = n.Compress;
            this.CompressDirectory = n.CompressDirectory;
            this.CompressQuality = n.CompressQuality;
            this.DeleteImages = n.DeleteImages;
            this.Directory = n.Directory;
            this.Render = n.Render;
            this.Title = n.Title;
            this.UpdateFrequency = n.UpdateFrequency;
        }
    }
}

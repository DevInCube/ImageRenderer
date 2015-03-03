using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VitML.ImageRenderer.Configurations
{
    public interface IObjectProvider
    {
        T GetObject<T>(string key);

        void AddObject<T>(T Storage);
    }
}

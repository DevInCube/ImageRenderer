using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using VitML.ImageRenderer.Core;

namespace VitML.ImageRenderer.Storages
{
    public class HTTPStorage : ImageStorage
    {

        public HTTPStorage()
        {
            //
        }

        public override void Connect()
        {
            throw new NotImplementedException();
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<string> GetImages()
        {
            throw new NotImplementedException();
        }

        public override bool IsAvailable(string id)
        {
            throw new NotImplementedException();
        }

        public override BitmapImage Load(string id)
        {
            throw new NotImplementedException();
        }

        public override bool Save(string id, System.Windows.Media.Imaging.BitmapImage image)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(string id)
        {
            throw new NotImplementedException();
        }
    }
}

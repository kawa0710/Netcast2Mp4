using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web.Models;

namespace Web.ViewModels
{
    public class ImageViewModel : Image
    {
        public string Name
        {
            get
            {
                return ImgGuid16 + ImgExt;
            }
        }

        public string Base64 { get; set; }
    }
}

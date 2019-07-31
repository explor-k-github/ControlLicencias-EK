using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlLicencias.Models {
    public class RequestImage {
        public IFormFile photo { set; get; }
    }
}
